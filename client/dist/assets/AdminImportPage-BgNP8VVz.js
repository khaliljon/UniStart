import{b as x,r as n,j as r,Z as u}from"./index-CG14Y6Jn.js";function f(){const{t:e}=x(),[i,l]=n.useState(""),[o,a]=n.useState(null),[m,d]=n.useState(!1),c=async()=>{try{d(!0),a(null);const t=JSON.parse(i),s=Array.isArray(t)?t:t.questions,p=await u.bulkImport(s);a(p)}catch(t){a({imported:0,failed:0,errors:[`JSON parse error: ${t}`]})}finally{d(!1)}};return r.jsxs("div",{className:"animate-fade-in",style:{padding:"2rem 0"},children:[r.jsx("h1",{style:{marginBottom:"0.5rem"},children:e.admin.import.title}),r.jsx("p",{style:{color:"var(--text-secondary)",marginBottom:"1.5rem"},children:e.admin.import.subtitle}),r.jsxs("div",{className:"card",style:{padding:"1.5rem"},children:[r.jsxs("p",{style:{color:"var(--text-secondary)",fontSize:"0.85rem",marginBottom:"1rem"},children:[e.admin.import.instructions," ",r.jsx("code",{children:"topicId"}),", ",r.jsx("code",{children:"text"}),", ",r.jsx("code",{children:"difficulty"}),", ",r.jsx("code",{children:"answerOptions[]"}),"."]}),r.jsx("textarea",{value:i,onChange:t=>l(t.target.value),rows:14,placeholder:`[
  {
    "topicId": 1,
    "text": "What is 2+2?",
    "difficulty": "Easy",
    "explanation": "Basic addition",
    "answerOptions": [
      { "text": "3", "isCorrect": false },
      { "text": "4", "isCorrect": true },
      { "text": "5", "isCorrect": false },
      { "text": "6", "isCorrect": false }
    ]
  }
]`,style:{width:"100%",fontFamily:"monospace",fontSize:"0.85rem",padding:"0.75rem",borderRadius:"0.5rem",border:"1px solid var(--border-color)",background:"var(--background-color)",color:"var(--text-primary)",resize:"vertical"}}),r.jsx("button",{className:"btn btn-primary",style:{marginTop:"1rem"},disabled:m||!i.trim(),onClick:c,children:m?e.admin.import.importing:e.admin.import.importBtn}),o&&r.jsxs("div",{style:{marginTop:"1rem",padding:"1rem",borderRadius:"0.5rem",background:o.failed>0?"rgba(239,68,68,0.08)":"rgba(16,185,129,0.08)",border:`1px solid ${o.failed>0?"var(--error-color)":"var(--success-color)"}`},children:[r.jsxs("div",{style:{fontWeight:600,marginBottom:"0.5rem"},children:[e.admin.import.imported," ",o.imported," / ",o.imported+o.failed]}),o.errors.length>0&&r.jsx("div",{style:{fontSize:"0.85rem",color:"var(--error-color)"},children:o.errors.map((t,s)=>r.jsxs("div",{children:["• ",t]},s))})]})]}),r.jsxs("div",{className:"card",style:{padding:"1.5rem",marginTop:"1rem"},children:[r.jsx("h3",{style:{marginBottom:"0.75rem"},children:e.admin.import.formatTitle}),r.jsx("pre",{style:{background:"var(--background-color)",padding:"1rem",borderRadius:"0.5rem",fontSize:"0.8rem",overflow:"auto",color:"var(--text-primary)"},children:`{
  "topicId": number,       ${e.admin.import.commentTopicId}
  "text": string,          ${e.admin.import.commentQuestionText}
  "difficulty": string,    ${e.admin.import.commentDifficulty}
  "explanation": string?,  ${e.admin.import.commentExplanation}
  "answerOptions": [       ${e.admin.import.commentOptions}
    { "text": string, "isCorrect": boolean }
  ]
}`}),r.jsx("p",{style:{color:"var(--text-secondary)",fontSize:"0.8rem",marginTop:"0.75rem"},children:"IRT-параметры (b, a, c) рассчитываются автоматически на основе difficulty."})]})]})}export{f as default};
