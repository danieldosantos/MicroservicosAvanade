const test = require('node:test');
const assert = require('node:assert');
const InventoryService = require('./inventoryService');

test('InventoryService creates products and validates stock', () => {
  const service = new InventoryService();
  service.createProduct({ id: 'p1', name: 'Product 1', stock: 10 });
  assert.strictEqual(service.validateStock('p1', 5), true);
  assert.strictEqual(service.validateStock('p1', 15), false);
});

test('InventoryService throws when creating duplicate products', () => {
  const service = new InventoryService();
  service.createProduct({ id: 'p1', name: 'Product 1', stock: 10 });
  assert.throws(() => service.createProduct({ id: 'p1', name: 'Product 1', stock: 5 }), /Product already exists/);
});
