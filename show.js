const fs=require('fs'
const text=fs.readFileSync('Controllers\\PlatosController.cs','utf8'); 
const start=text.indexOf('CreateSimple'); 
const end=text.indexOf('}',text.indexOf('CreateSimple'))+1; 
console.log(text.slice(start,end));
