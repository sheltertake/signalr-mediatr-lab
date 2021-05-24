import { Injectable } from '@angular/core';
import * as signalR from "@microsoft/signalr"; 
// import { MessagePackHubProtocol } from '@microsoft/signalr-protocol-msgpack';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {

  private hubConnection!: signalR.HubConnection;  
  private counter = 0;
  public startConnection = () => {
    this.hubConnection = new signalR.HubConnectionBuilder()
                            .withUrl('http://localhost:5000/notifications')
                            // .withHubProtocol(new MessagePackHubProtocol())
                            .configureLogging(signalR.LogLevel.Information)
                            .build();
    this.hubConnection
      .start()
      .then(() => 
      {
        // console.log('Connection started');
        // console.log("Invoke SendPingNotificationAsync", this.counter)
        // this.hubConnection.invoke('SendPingNotificationAsync', '1 Frontend send message');

        console.log('Invoke RoverLandRequestAsync');
        this.hubConnection.invoke('RoverLandRequestAsync');
      })
      .catch(err => console.log('Error while starting connection: ' + err))
  }
  public registerOnServerEvents(){
    // this.hubConnection.on(
    //   'SendPongNotificationAsync',
    //   (data: any) => {
    //       console.log('Listening SendPongNotificationAsync - received', data, ++this.counter);
    //       setTimeout(() =>{
    //         console.log("Invoke SendPingNotificationAsync", this.counter)
    //         this.hubConnection.invoke('SendPingNotificationAsync',  this.counter + ' Frontend send message');
    //       }, 1000);          
    //   });

    this.hubConnection.on(
      'RoverLandResponseAsync',
      (data: any) => {
          console.log('Listening RoverLandResponseAsync - received', data);
      });
  }
}
