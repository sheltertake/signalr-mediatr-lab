import { Injectable } from '@angular/core';
import * as signalR from "@microsoft/signalr"; 

@Injectable({
  providedIn: 'root'
})
export class SignalRService {

  private hubConnection!: signalR.HubConnection;  
  private counter = 0;
  public startConnection = () => {
    this.hubConnection = new signalR.HubConnectionBuilder()
                            .withUrl('http://localhost:5000/notifications')
                            .build();
    this.hubConnection
      .start()
      .then(() => 
      {
        console.log('Connection started');
        console.log("Invoke SendPingNotification sending: Frontend send message", this.counter)
        this.hubConnection.invoke('SendPingNotification', '1 Frontend send message');
      })
      .catch(err => console.log('Error while starting connection: ' + err))
  }
  public registerOnServerEvents(){
    this.hubConnection.on(
      'SendPongNotification',
      (data: any) => {
          console.log('Listening SendPongNotification - received', data, ++this.counter);
          setTimeout(() =>{
            this.hubConnection.invoke('SendPingNotification',  this.counter + ' Frontend send message');
          }, 1000);          
      });
  }
}
