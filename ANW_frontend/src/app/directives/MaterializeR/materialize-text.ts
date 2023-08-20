declare var M:any;
export const MaterializeText ={
  render: (delay: number=100)=>{
    setTimeout(()=>{
      M.updateTextFields();
    },delay)
  }
}
