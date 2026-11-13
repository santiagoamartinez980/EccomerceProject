// Exposes only the Wompi webhook through a temporary tunnel.
const http = require('node:http');

const port = Number(process.env.WOMPI_PROXY_PORT || 5055);
const upstream = 'http://127.0.0.1:5000/api/Payment/webhook';

http.createServer(async (request, response) => {
  if (request.method !== 'POST' || request.url !== '/api/Payment/webhook') {
    response.writeHead(404).end();
    return;
  }

  try {
    const chunks = [];
    let size = 0;
    for await (const chunk of request) {
      size += chunk.length;
      if (size > 1024 * 1024) {
        response.writeHead(413).end();
        return;
      }
      chunks.push(chunk);
    }

    const result = await fetch(upstream, {
      method: 'POST',
      headers: {
        'content-type': request.headers['content-type'] || 'application/json',
        ...(request.headers['x-event-checksum']
          ? { 'x-event-checksum': request.headers['x-event-checksum'] }
          : {}),
      },
      body: Buffer.concat(chunks),
    });
    response.writeHead(result.status, { 'content-type': 'text/plain' });
    response.end(await result.text());
  } catch {
    response.writeHead(502).end();
  }
}).listen(port, '127.0.0.1', () => {
  process.stdout.write(`Wompi webhook proxy listening on 127.0.0.1:${port}\n`);
});
