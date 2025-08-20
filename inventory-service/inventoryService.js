class InventoryService {
  constructor() {
    this.products = new Map();
  }

  createProduct(product) {
    if (this.products.has(product.id)) {
      throw new Error('Product already exists');
    }
    this.products.set(product.id, { ...product });
  }

  validateStock(productId, quantity) {
    const product = this.products.get(productId);
    if (!product) return false;
    return product.stock >= quantity;
  }
}

module.exports = InventoryService;
