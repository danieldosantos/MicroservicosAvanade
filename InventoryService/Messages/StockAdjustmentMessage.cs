namespace InventoryService.Messages
{
    public class StockAdjustmentMessage
    {
        public int ProductId { get; set; }
        public int QuantitySold { get; set; }
    }
}
