import { trigger, transition, style, query, animate, stagger } from "../../../../node_modules/@angular/animations";

export const ListTransition=trigger("listanimation",[
    transition("* => *",[
        query(":leave",[
            style({opacity: 1, transform:'translateX(0)'}),
            stagger(20,[
                animate("0.1s linear", style({ opacity:0, transform: 'translateX(80px)'}))
            ]),
        ], {optional: true}),
        query(":enter",[
            style({opacity: 0, transform:'translateX(100px)'}),
            stagger(50,[
                animate("0.15s linear", style({ opacity:1, transform: 'translateX(0)'}))
            ])
        ], {optional: true})
    ])
])