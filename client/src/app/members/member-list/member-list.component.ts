import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { Pagination } from 'src/app/_models/pagination';
import { User } from 'src/app/_models/user';
import { UserParams } from 'src/app/_models/userParams';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {

  // members$: Observable<Member[]> | undefined;
  members: Member[] | undefined;
  pagination: Pagination | undefined;
  userParams: UserParams | undefined;
  user: User | undefined;


  constructor(private memberService: MembersService, private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      if (user) {
        this.user = user;
        this.userParams = new UserParams(user);
      }
      else {
        console.log('user is null');
      }
    })
   }

  ngOnInit(): void {
    // this.members$ = this.memberService.getMembers();
    this.loadMembers();
  }

  loadMembers() {
    if (this.userParams) {
      this.memberService.getMembers(this.userParams).subscribe(response => {
        this.members = response.result;
        this.pagination = response.pagination;
    })
    }
    else {
      console.log('user params is null')
    }

  }

  pageChanged(event: any) {
    if (this.userParams) {
      this.userParams.pageNumber = event.page;
      this.loadMembers();
    }
    else {
      console.log('user params is null')
    }
  }


}
