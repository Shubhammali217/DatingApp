import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {

  model: any = {}
  currentUser$: Observable<User>;
  constructor(private accountService: AccountService) { }

  ngOnInit(): void {
    this.currentUser$ = this.accountService.currenUser$;
  }

  login() {
    this.accountService.login(this.model).subscribe(response => {
      console.log(response);
    }, error => {
      console.log(error);
      alert("Please enter correct username pass!!")
    })
    console.log(this.model);
  }

  logout() {
    this.accountService.logout();
  }

}
