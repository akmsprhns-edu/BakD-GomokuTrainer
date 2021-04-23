import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { GomokuUiComponent } from './gomoku-ui/gomoku-ui.component';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,

    GomokuUiComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot(
      [
        //{ path: '', component: HomeComponent, pathMatch: 'full' },
        { path: 'GomokuUI', component: GomokuUiComponent }
      ]
    )
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
