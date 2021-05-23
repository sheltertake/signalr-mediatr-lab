import { Component } from '@angular/core';
import { SignalRService } from './services/signal-r.service';

@Component({
  selector: 'app-root',
  template: `
    Hello world
  `,
  styles: []
})
export class AppComponent {
 
   constructor(public signalRService: SignalRService) { }

   ngOnInit() {
     this.signalRService.startConnection();
     this.signalRService.registerOnServerEvents();
   }
}
