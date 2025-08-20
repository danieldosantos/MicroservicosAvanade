class SalesService {
  constructor(inventoryService, mqChannel) {
    this.inventoryService = inventoryService;
    this.mqChannel = mqChannel;
  }

  async createOrder(order) {
    if (!this.inventoryService.validateStock(order.productId, order.quantity)) {
      throw new Error('Insufficient stock');
    }
    await this.mqChannel.sendToQueue('orders', Buffer.from(JSON.stringify(order)));
    return { status: 'created' };
  }
}

module.exports = SalesService;
