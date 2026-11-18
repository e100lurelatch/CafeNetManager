using System.Configuration;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CafeNetManager.Services
{
    // Module 02 - Bảo: Thanh toán PayPal SANDBOX (tiền ảo, không mất tiền thật)
    // Tài liệu: https://developer.paypal.com/docs/api/orders/v2/
    // Luồng: lấy access token -> tạo order -> khách duyệt trên trang PayPal -> capture (chốt tiền)
    public class PayPalService
    {
        private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(30) };

        public string ClientId { get; }
        public string Secret { get; }
        public string BaseUrl { get; }
        public string ReturnUrl { get; }
        public string CancelUrl { get; }
        public string Currency { get; }
        public decimal VndPerUnit { get; }      // tỷ giá quy đổi VND -> USD (PayPal không hỗ trợ VND)

        public PayPalService()
        {
            var s = ConfigurationManager.AppSettings;
            ClientId  = s["PayPal_ClientId"] ?? "";
            Secret    = s["PayPal_Secret"] ?? "";
            BaseUrl   = (s["PayPal_BaseUrl"] ?? "https://api-m.sandbox.paypal.com").TrimEnd('/');
            ReturnUrl = s["PayPal_ReturnUrl"] ?? "http://localhost/paypal_return";
            CancelUrl = s["PayPal_CancelUrl"] ?? "http://localhost/paypal_return/cancel";
            Currency  = s["PayPal_Currency"] ?? "USD";
            VndPerUnit = decimal.TryParse(s["PayPal_VndPerUsd"], NumberStyles.Number, CultureInfo.InvariantCulture, out var r) && r > 0
                ? r : 25000m;
        }

        /// <summary>Đã điền Client ID / Secret thật chưa.</summary>
        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(Secret)
            && !ClientId.StartsWith("DIEN_") && !Secret.StartsWith("DIEN_");

        /// <summary>Đổi VND sang USD, làm tròn 2 số lẻ, tối thiểu 0.01.</summary>
        public decimal ConvertFromVnd(decimal vnd) => Math.Max(0.01m, Math.Round(vnd / VndPerUnit, 2, MidpointRounding.AwayFromZero));

        // ===== 1. Lấy access token (OAuth 2.0 client credentials) =====
        private async Task<string> GetAccessTokenAsync()
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v1/oauth2/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["grant_type"] = "client_credentials" })
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ClientId}:{Secret}")));

            var json = await SendAsync(req, "lấy access token");
            return json["access_token"]?.GetValue<string>()
                   ?? throw new Exception("PayPal không trả về access_token.");
        }

        // ===== 2. Tạo order, trả về (orderId, link duyệt thanh toán) =====
        public async Task<(string OrderId, string ApproveUrl)> CreateOrderAsync(decimal amountVnd, string referenceId, string description)
        {
            var token = await GetAccessTokenAsync();
            var value = ConvertFromVnd(amountVnd).ToString("0.00", CultureInfo.InvariantCulture);

            var body = new JsonObject
            {
                ["intent"] = "CAPTURE",
                ["purchase_units"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["reference_id"] = referenceId,
                        ["description"] = description,
                        ["amount"] = new JsonObject { ["currency_code"] = Currency, ["value"] = value }
                    }
                },
                ["payment_source"] = new JsonObject
                {
                    ["paypal"] = new JsonObject
                    {
                        ["experience_context"] = new JsonObject
                        {
                            ["brand_name"] = "CafeNetManager",
                            ["shipping_preference"] = "NO_SHIPPING",
                            ["user_action"] = "PAY_NOW",
                            ["landing_page"] = "LOGIN",        // mở trang đăng nhập PayPal trước (không vào thẳng form thẻ)
                            ["return_url"] = ReturnUrl,
                            ["cancel_url"] = CancelUrl
                        }
                    }
                }
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v2/checkout/orders")
            {
                Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json")
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Headers.Add("PayPal-Request-Id", referenceId);      // chống tạo trùng order khi bấm 2 lần

            var json = await SendAsync(req, "tạo order");
            var orderId = json["id"]?.GetValue<string>() ?? throw new Exception("PayPal không trả về mã order.");

            // Link cho khách duyệt: "payer-action" (payment_source) hoặc "approve" (kiểu cũ)
            var approve = json["links"]?.AsArray()
                .FirstOrDefault(l => l?["rel"]?.GetValue<string>() is "payer-action" or "approve")?["href"]?.GetValue<string>()
                ?? throw new Exception("PayPal không trả về link duyệt thanh toán.");

            return (orderId, approve);
        }

        // ===== 3. Capture (chốt tiền) sau khi khách đã duyệt =====
        public async Task<PayPalResult> CaptureOrderAsync(string orderId)
        {
            var token = await GetAccessTokenAsync();
            using var req = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v2/checkout/orders/{orderId}/capture")
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var json = await SendAsync(req, "capture order");
            var status = json["status"]?.GetValue<string>() ?? "";
            var capture = json["purchase_units"]?[0]?["payments"]?["captures"]?[0];

            return new PayPalResult
            {
                OrderId = orderId,
                Status = status,
                IsSuccess = status == "COMPLETED" && capture?["status"]?.GetValue<string>() == "COMPLETED",
                CaptureId = capture?["id"]?.GetValue<string>() ?? "",
                Amount = capture?["amount"]?["value"]?.GetValue<string>() ?? "",
                Currency = capture?["amount"]?["currency_code"]?.GetValue<string>() ?? Currency
            };
        }

        /// <summary>Đọc URL PayPal chuyển về: lấy mã order (token) và biết khách có bấm hủy không.</summary>
        public (bool Cancelled, string? OrderId) ParseReturnUrl(string url)
        {
            var uri = new Uri(url);
            var q = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var cancelled = url.StartsWith(CancelUrl, StringComparison.OrdinalIgnoreCase);
            return (cancelled, q["token"]);
        }

        // Gửi request, lỗi thì báo rõ mã HTTP + thông báo của PayPal
        private static async Task<JsonNode> SendAsync(HttpRequestMessage req, string step)
        {
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            using var res = await Http.SendAsync(req);
            var text = await res.Content.ReadAsStringAsync();

            JsonNode? json = null;
            try { json = JsonNode.Parse(text); } catch (JsonException) { }

            if (!res.IsSuccessStatusCode || json == null)
            {
                var detail = json?["error_description"]?.GetValue<string>()
                          ?? json?["message"]?.GetValue<string>()
                          ?? (text.Length > 300 ? text[..300] + "..." : text);
                throw new Exception($"PayPal lỗi khi {step} (HTTP {(int)res.StatusCode}): {detail}");
            }
            return json;
        }
    }

    public class PayPalResult
    {
        public bool IsSuccess { get; set; }
        public string OrderId { get; set; } = "";
        public string CaptureId { get; set; } = "";
        public string Status { get; set; } = "";
        public string Amount { get; set; } = "";
        public string Currency { get; set; } = "USD";
    }
}
