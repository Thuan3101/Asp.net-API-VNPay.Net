using Microsoft.EntityFrameworkCore;
using VNPAY.NET.Models;

namespace VNPAY.NET
{
    public class VnpayDbContext : DbContext
    {
        public VnpayDbContext(DbContextOptions<VnpayDbContext> options) : base(options) { }

        public DbSet<Payment> Payments { get; set; }


        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    // Cấu hình PaymentId là khóa chính
        //    modelBuilder.Entity<PaymentTransaction>()
        //        .HasKey(p => p.PaymentId);
        //}

    }

    //public class PaymentTransaction
    //{
    //    public long PaymentId { get; set; }
    //    public bool IsSuccess { get; set; }
    //    public string Description { get; set; }
    //    public DateTime Timestamp { get; set; }
    //    public long VnpayTransactionId { get; set; }
    //    public string PaymentMethod { get; set; }
    //    public int PaymentResponseCode { get; set; }
    //    public string PaymentResponseDescription { get; set; }
    //    public int TransactionStatusCode { get; set; }
    //    public string TransactionStatusDescription { get; set; }
    //    public string BankCode { get; set; }
    //    public string BankTransactionId { get; set; }
    //}
}
