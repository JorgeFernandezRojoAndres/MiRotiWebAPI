const fs=require('node:fs');
const text=fs.readFileSync('Controllers/ControllersApi/PedidosApiController.cs','utf8');
const lines=text.split(String.fromCharCode(10));
