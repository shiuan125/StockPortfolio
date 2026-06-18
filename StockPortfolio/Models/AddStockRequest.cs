using System.ComponentModel.DataAnnotations;

namespace StockPortfolio.Models
{
    public class AddStockRequest
    {
        [Required(ErrorMessage = "請輸入股票代號")]
        public string Symbol { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入股數")]
        [Range(0.001, double.MaxValue, ErrorMessage = "股數必須大於 0")]
        public decimal Shares { get; set; }

        [Required(ErrorMessage = "請輸入購入成本")]
        [Range(0.001, double.MaxValue, ErrorMessage = "購入成本必須大於 0")]
        public decimal PurchasePrice { get; set; }

        [Required(ErrorMessage = "請選擇購入日期")]
        public DateTime PurchaseDate { get; set; } = DateTime.Today;
    }
}
