import { Directive, Input, AfterViewInit, ElementRef, OnDestroy } from '@angular/core';
import * as Chart from '../../../assets/chartjs/Chart.min.js';

@Directive({
  selector: '[chartjs]'
})
export class ChartJsDirective implements AfterViewInit, OnDestroy {

  @Input('chartjs') chartType: string='line';
  @Input('chartinfo-position') legendPosition: string='top';
  @Input('chartinfo-reverse') legendReverse: boolean=false;
  @Input('chartinfo-hide') legendHide: boolean=false;
  @Input('chart-scale') scale: string[];
  @Input('chart-dataset') datasets: ChartDataset[];
  @Input('chart-xlabel') xlabel: string;
  @Input('chart-ylabel') ylabel: string;
  @Input('chart-title') title: string;
  @Input('chart-min') min: number=0;
  @Input('chart-max') max: number;
  @Input('chart-options') optional: any= ChartOption;
  @Input('chart-font-color') fontColor: string= "grey";
  @Input('chart-grid-color') gridColor: string= "grey";
  @Input('chart-line-color') lineColor: string= "grey";
  private config: any;
  private chart: any;
  private static colors: string[]=[ 'rgb(255,155,155)', 'rgb(155,222,255)','rgb(190,190,190)','lightgreen','gold','springgreen','lightyellow']
  private static colorsFill: string[]=[ 'rgba(255,155,155,0.2)', 'rgba(155,222,255,0.2)','rgba(190,190,190,0.2)','rgba(144,238,144,0.2)',
      'rgba(255,215,0,0.2)','rgba(0,255,128,0.2)','rgba(255,255,244,0.2)']
  constructor(private el: ElementRef) { }

  ngAfterViewInit(){
    const elem=this.el.nativeElement.getContext('2d');
    this.config=this.getConfig();
    if (this.fontColor)
      Chart.defaults.global.defaultFontColor=this.fontColor;
    this.chart=new Chart(elem,this.config);
  }

  getScale(){
    return this.config.data.labels;
  }
  setScale(scale: string[])
  {
    this.config.data.labels=scale;
    this.update();
  }
  getDatasets(){
    return this.config.data.datasets;
  }
  setDatasets(datasets: ChartDataset[]){
    this.config.data.datasets=datasets;
    this.update();
  }
  update(delay: number=10){
    setTimeout((()=>this.chart.update()).bind(this),delay);
    //this.chart.update();
  }

  private getConfig(){
    return {
      type: this.chartType?this.chartType:'line',
      data: {
        labels: this.scale,
        datasets: this.datasets
      },
      options: this.getOption(this.optional)
    }
  }

  getOption(optional){
    let option=optional;
    option['scales']={};
    if (this.xlabel){
      option['scales'].xAxes=[];
      option['scales'].xAxes[0]={};
      option['scales'].xAxes[0].gridLines ={};
      option['scales'].xAxes[0].gridLines.display =false;
      if (this.gridColor)
        option['scales'].xAxes[0].gridLines.color =this.gridColor;
      if (this.lineColor)
        option['scales'].xAxes[0].gridLines.zeroLineColor =this.lineColor;
      option['scales'].xAxes[0].display=true;
      option['scales'].xAxes[0].scaleLabel={};
      option['scales'].xAxes[0].scaleLabel.display=true;
      option['scales'].xAxes[0].scaleLabel.labelString=this.xlabel;
    }
    if (this.ylabel){
      option['scales'].yAxes=[];
      option['scales'].yAxes[0]={};
      option['scales'].yAxes[0].gridLines ={};
      option['scales'].yAxes[0].gridLines.display =false;
      if (this.gridColor)
        option['scales'].yAxes[0].gridLines.color =this.gridColor;
      if (this.lineColor)
        option['scales'].yAxes[0].gridLines.zeroLineColor =this.lineColor;
      option['scales'].yAxes[0].display=true;
      option['scales'].yAxes[0].scaleLabel={};
      option['scales'].yAxes[0].scaleLabel.display=true;
      option['scales'].yAxes[0].scaleLabel.labelString=this.ylabel;
    }
    if (this.min || this.max){
      if (!option['scales'].yAxes){
        option['scales'].yAxes=[];
        option['scales'].yAxes[0]={};
      }
      option['scales'].yAxes[0].ticks={};
      option['scales'].yAxes[0].ticks.suggestedMin=this.min;
      option['scales'].yAxes[0].ticks.suggestedMax=this.max;
    }
    if (this.title){
      option['title']={};
      option['title'].display=true;
      option['title'].text=this.title;
    }
    option['legend']={};
    option['legend'].display=!this.legendHide;
    if (this.fontColor){
      option['legend'].labels={ fontColor: this.fontColor };
    }
    else
      option['legend'].labels={};
    option['legend'].labels.usePointStyle=true;
    if (this.legendPosition){
      option['legend'].position=this.legendPosition;
    }
    if (this.legendReverse){
      option['legend'].reverse=this.legendReverse;
    }
    return option;
  }

  ngOnDestroy(){
    if (this.chart)
      this.chart.destroy();
  }

  static get defaultColors(){
    return this.colors;
  }
  static get defaultFillColor(){
    return this.colorsFill;
  }
}

export class ChartDataset{
  label: string;
  data: number[];
  backgroundColor: string | string[];
  borderColor?: string | string[];
  borderWidth?: number;
  fill?: boolean;
  pointRadius?: number | number[];
  hoverBackgroundColor?: string | string[];
  pointBackgroundColor?: string;
  hoverBorderColor?: string | string[];
  pointBorderColor?: string="white";
  pointBorderWidth?: number=2;
}
export var ChartOption={
  responsive: true,
  tooltips:{
    mode:'index',
    intersect: false
  },
  hover:{
    mode:'nearest',
    intersect: false,
    animationDuration: 250
  },
  animation:{
    duration: 350
  },
  responsiveAnimationDuration: 250
}