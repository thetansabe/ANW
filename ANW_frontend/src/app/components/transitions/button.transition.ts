import { trigger, transition, style, animate} from '@angular/animations';
export const ButtonTransition=trigger('button',[
        transition(':enter',[
            //Begin transition
                style({ opacity: 0, transform:'rotate(90deg)'}),
                animate("0.3s ease", style({ opacity: 1, transform:'rotate(0)'}))
            //End transition
        ])
    ]);