using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;
using VNPAY.NET.Utilities;

namespace Backend_API_Testing.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VnpayController : ControllerBase
    {
        private readonly IVnpay _vnpay;
        private readonly IConfiguration _configuration;
        private readonly VnpayDbContext _dbContext; // Replace `YourDbContext` with the actual name of your DbContext

        public VnpayController(IVnpay vnPayservice, IConfiguration configuration, VnpayDbContext dbContext)
        {
            _vnpay = vnPayservice;
            _configuration = configuration;
            _dbContext = dbContext;

            _vnpay.Initialize(_configuration["Vnpay:TmnCode"], _configuration["Vnpay:HashSecret"], _configuration["Vnpay:BaseUrl"], _configuration["Vnpay:CallbackUrl"]);
        }
        /// <summary>
        /// Lấy danh sách các giao dịch thanh toán
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <returns></returns>

        [HttpGet("GetPayments")]
        public async Task<IActionResult> GetPayments(int page = 1, int pageSize = 10, string sort = "createdDate")
        {
            try
            {
                var (payments, totalCount) = await _vnpay.GetPaymentsAsync(page, pageSize, sort);

                return Ok(new
                {
                    totalCount,
                    page,
                    pageSize,
                    data = payments
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while retrieving payments.", details = ex.Message });
            }
        }


        /// <summary>
        /// Tạo url thanh toán
        /// </summary>
        /// <param name="money">Số tiền phải thanh toán</param>
        /// <param name="description">Mô tả giao dịch</param>
        /// <returns></returns>
        [HttpGet("CreatePaymentUrl")]
        public async Task<ActionResult<string>> CreatePaymentUrl(double money, string description)
        {
            try
            {
                var ipAddress = NetworkHelper.GetIpAddress(HttpContext);

                var paymentRequest = new PaymentRequest
                {
                    PaymentId = DateTime.Now.Ticks,
                    Money = money,
                    Description = description,
                    IpAddress = ipAddress,
                    BankCode = BankCode.ANY, // Tùy chọn. Mặc định là tất cả phương thức giao dịch
                    CreatedDate = DateTime.Now, // Tùy chọn. Mặc định là thời điểm hiện tại
                    Currency = Currency.VND, // Tùy chọn. Mặc định là VND (Việt Nam đồng)
                    Language = DisplayLanguage.Vietnamese // Tùy chọn. Mặc định là tiếng Việt
                };

                // Tạo URL thanh toán
                var paymentUrl = _vnpay.GetPaymentUrl(paymentRequest);

                // Lưu thông tin giao dịch vào CSDL
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var payment = new Payment
                    {
                        PaymentId = paymentRequest.PaymentId,
                        Description = paymentRequest.Description,
                        Amount = paymentRequest.Money,
                        CreatedDate = paymentRequest.CreatedDate,
                        IpAddress = paymentRequest.IpAddress,
                        IsSuccess = false,
                        TransactionStatus = "Pending", // Trạng thái ban đầu
                        PaymentMethod = "VNPAY"
                    };

                    await _dbContext.Payments.AddAsync(payment);
                    await _dbContext.SaveChangesAsync();
                    scope.Complete();
                }

                return Ok(paymentUrl);
            }
            catch (DbUpdateException dbEx)
            {
                if (dbEx.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + dbEx.InnerException.Message);
                    Console.WriteLine("StackTrace: " + dbEx.StackTrace);
                }
                return BadRequest("An error occurred while saving the entity changes. See the inner exception for details.");
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                    Console.WriteLine("StackTrace: " + ex.StackTrace);
                }
                return BadRequest(ex.Message);
            }
        }



        /// <summary>
        /// Thực hiện hành động sau khi thanh toán. URL này cần được khai báo với VNPAY trước (ví dụ: http://localhost:1234/api/Vnpay/IpnAction)
        /// </summary>
        /// <returns></returns>
        [HttpGet("IpnAction")]
        public IActionResult IpnAction()
        {
            if (Request.QueryString.HasValue)
            {
                try
                {
                    var paymentResult = _vnpay.GetPaymentResult(Request.Query);
                    if (paymentResult.IsSuccess)
                    {
                        // Thực hiện hành động nếu thanh toán thành công tại đây. Ví dụ: Cập nhật trạng thái đơn hàng trong cơ sở dữ liệu.
                        return Ok();
                    }

                    // Thực hiện hành động nếu thanh toán thất bại tại đây. Ví dụ: Hủy đơn hàng.
                    return BadRequest("Thanh toán thất bại");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return NotFound("Không tìm thấy thông tin thanh toán.");
        }

        /// <summary>
        /// Trả kết quả thanh toán về cho người dùng
        /// </summary>
        /// <returns></returns>
        [HttpGet("Callback")]
        public async Task<IActionResult> PaymentCallback()
        {
            try
            {
                var parameters = HttpContext.Request.Query;
                var paymentResult = _vnpay.GetPaymentResult(parameters);

                var payment = await _dbContext.Payments
                    .FirstOrDefaultAsync(p => p.PaymentId == paymentResult.PaymentId);

                if (payment != null)
                {
                    payment.IsSuccess = paymentResult.IsSuccess;
                    payment.TransactionStatus = paymentResult.TransactionStatus.Description;
                    payment.PaymentMethod = paymentResult.PaymentMethod;

                    _dbContext.Payments.Update(payment);
                    await _dbContext.SaveChangesAsync();
                }

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}