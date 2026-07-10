import{h as a}from"./index-BgEGt7na.js";const r={async list(){return(await a.get("/purchases")).data},async checkout(s){return(await a.post("/purchases/checkout",s)).data}};export{r as p};
