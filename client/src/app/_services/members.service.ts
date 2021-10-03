import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';



@Injectable({
  providedIn: 'root'
})
export class MembersService {

  baseUrl: string = environment.apiUrl;
  members: Member[] = [];
  memberCache = new Map();
  user!: User;
  userParams!: UserParams;

  constructor(private http: HttpClient, private accountService: AccountService) {
      this.accountService.currentUser$.pipe(take(1)).subscribe((user )=> {
        if (user) {
          this.user = user;
          this.userParams = new UserParams(user);
        }
        else {
          console.log('user is null');
        }
      })
   }

   getUserParams() {
     return this.userParams;
   }

   setUserParams(params: UserParams) {
     this.userParams = params;
   }

   resetUserParams() {
     this.userParams = new UserParams(this.user);
     return this.userParams;
   }

  getMembers(userParams: UserParams) {

    var response = this.memberCache.get(Object.values(userParams).join('-'));
    if (response) {
      return of(response);
    }

    let params = this.getPaginationHeaders(userParams.pageNumber, userParams.pageSize)

    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);

    return this.getPaginatedResult<Member[]>(this.baseUrl + 'users', params)
      .pipe(map(response => {
        this.memberCache.set(Object.values(userParams).join('-'), response);
        return response;
      }))
  }


  getMember(username: string) {
    const arrayResultOfMap = [...this.memberCache.values()]
      .reduce((array, element) => array.concat(element.result), []); // flattened members into an array

    const member = arrayResultOfMap.find((member: Member) => member.username === username);

    if (member) {
      return of(member);
    }

    return this.http.get<Member>(this.baseUrl + 'users/' + username)
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = member;
      })
    )
  }

  setMainPhoto(photoId: number) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photoId, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photoId);
  }

  private getPaginatedResult<T>(url: string, params: HttpParams) {

    const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();

    return this.http.get<T>(url, { observe: 'response', params }).pipe(
      map(response => {

        if (response.body !== null) {
          paginatedResult.result = response.body; // members array

          const pagination = response.headers.get('Pagination');
          if (pagination !== null) {
            paginatedResult.pagination = JSON.parse(pagination);

          }
        }
        return paginatedResult;
      })
    );
  }

  private getPaginationHeaders(pageNumber: number, pageSize: number) {

    let params = new HttpParams();

    params = params.append('pageNumber', pageNumber.toString());
    params = params.append('pageSize', pageSize.toString());

    return params;
  }

}


