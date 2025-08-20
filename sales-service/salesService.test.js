const test = require('node:test');
const assert = require('node:assert');
const SalesService = require('./salesService');

test('SalesService creates order when stock is sufficient', async () => {
  const inventoryMock = { validateStock: () => true };
  let sent;
  const channelMock = {
    sendToQueue: (queue, msg) => {
      sent = { queue, msg };
    }
  };
  const service = new SalesService(inventoryMock, channelMock);
  const order = { productId: 'p1', quantity: 2 };
  await service.createOrder(order);
  assert.deepStrictEqual(sent, { queue: 'orders', msg: Buffer.from(JSON.stringify(order)) });
});

test('SalesService throws error when stock is insufficient', async () => {
  const inventoryMock = { validateStock: () => false };
  let sendCalled = false;
  const channelMock = {
    sendToQueue: () => {
      sendCalled = true;
    }
  };
  const service = new SalesService(inventoryMock, channelMock);
  const order = { productId: 'p1', quantity: 2 };
  await assert.rejects(() => service.createOrder(order), /Insufficient stock/);
  assert.strictEqual(sendCalled, false);
});
