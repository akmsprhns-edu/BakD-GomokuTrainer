import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { environment } from 'src/environments/environment';

enum StoneColor {
  First = 1,
  Second = 2,
  None = 0
}

enum GameResult {
  Tie = 0,
  FirstPlayerWon = 1,
  SecondPlayerWon = 2
}

@Component({
  selector: 'app-gomoku-ui',
  templateUrl: './gomoku-ui.component.html',
  styleUrls: ['./gomoku-ui.component.scss']
})
export class GomokuUiComponent implements OnInit {

  public letters : string[] = ["A", "B", "C", "D", "E", "F", "G", "H", "I", "J" , "K", "L", "M", "N", "O", "P"]
  public playerType = StoneColor;
  public gameResultType = GameResult;
  public gameSession: GameSession;
  public moveInProgress = false;
  private _httpClient: HttpClient
  constructor(httpClient: HttpClient) {
    this._httpClient = httpClient;
    this._httpClient.post<GameSession>(environment.baseUrl + '/Api/Gomoku/NewGame', null).subscribe(result => {
      this.gameSession = result
    }, error => console.error(error));
  }

  ngOnInit(): void {
  }

  public makeMove(row: number, col: number):void {
    if(this.moveInProgress == true || this.gameSession.gameResult != null) 
      return;
    this.moveInProgress = true;
    if(this.gameSession.board[row][col] != StoneColor.None){
      return;
    } else {
      this.gameSession.board[row][col] = StoneColor.First;
      let request = <MoveRequest>{
        gameSessionGuid: this.gameSession.guid,
        row: row,
        column: col
      };
      this._httpClient.post<GameSession>(environment.baseUrl + '/Api/Gomoku/MakeMove', request).subscribe(
        result => {
        this.gameSession = result;
        this.moveInProgress = false;
        }, 
        error => { 
          console.error(error);
          this.gameSession.board[row][col] = StoneColor.None;
          this.moveInProgress = false;
        }
      );
    }
  }

}

interface GameSession {
  guid: string;
  board: StoneColor[][];
  gameResult?: GameResult;
}

interface MoveRequest {
  gameSessionGuid: string;
  row: number;
  column: number;
}
