import { trigger, transition, query, group, style, animate} from '@angular/animations';
export const SimpleTransition=trigger('transition',[
        transition(':enter',[
            //Begin transition
                style({ opacity: 0, transform:'translateX(75px)', overflowY:'hidden'}),
                animate("0.15s 50ms ease", style({ opacity: 1, transform:'translateX(0px)'}))
            
            //End transition
        ])
    ]);