import{h as a}from"./index-BcfsvaAB.js";const r={async list(){return(await a.get("/purchases")).data},async checkout(s){return(await a.post("/purchases/checkout",s)).data}};export{r as p};
