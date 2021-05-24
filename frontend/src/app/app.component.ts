import { Component, HostListener, Injectable, Pipe, PipeTransform } from '@angular/core';
import { Observable, of } from 'rxjs';
import { SignalRService } from './services/signal-r.service';

enum KEY_CODE {
  ARROW_RIGHT = 'ArrowRight',
  ARROW_LEFT = 'ArrowLeft'
}
@Component({
  selector: 'app-root',
  template: `
    <div class="container">
      <div class="row" *ngFor="let row of map$ | async">
        <div class="square" *ngFor="let cell of row">{{cell | tris}}</div>
      </div>
    </div>    
  `,
  styles: [`
    :host{height:100vh}
  `]
})
export class AppComponent {

  map$: Observable<any> | undefined;
  map2$: Observable<any> | undefined;

  constructor(
    private signalRService: SignalRService,
    private simpleService: SimpleService
  ) { }

  ngOnInit() {
    this.signalRService.startConnection();
    this.signalRService.registerOnServerEvents();

    this.map$ = this.simpleService.Get();
  }
  
  @HostListener('window:keyup', ['$event'])
  keyEvent(event: KeyboardEvent) {
    console.log(event.key);

    if (event.key === KEY_CODE.ARROW_RIGHT) {
      this.map$ = this.simpleService.Get(true);
    }
  }
}
@Pipe({ name: 'tris' })
export class TrisPipe implements PipeTransform {
  transform(value: boolean | null): string {
    return value === true ? "O" :
      value === false ? "X" : " ";
  }
}
@Injectable({
  providedIn: 'root'
})
export class SimpleService {

  Get(testRight: boolean = false): Observable<any> {
    const map = [
      [false, null, false],
      [null, true, null],
      [false, null, false]
    ];

    if(testRight){
      console.log('testRight', map);
      map[1][1]=null;
      map[1][2]=true;
      console.log(map)
    }

    return of(map);
  }
}