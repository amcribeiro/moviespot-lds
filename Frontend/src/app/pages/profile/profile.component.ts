import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserResponseDto, UserUpdateDto } from '../../models/user.model';
import { UserService } from '../../services/user.service';
import { AuthTokenService } from '../../services/auth-token.service';
import { SidebarComponent } from '../../pages/sidebar/sidebar.component';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, SidebarComponent], 
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {

  user!: UserResponseDto;
  isEditing = false;

  private userService = inject(UserService);
  private authToken = inject(AuthTokenService);

  ngOnInit(): void {
    const id = this.authToken.getUserId();
    if (!id) return;

    this.userService.getById(id).subscribe({
      next: (res) => this.user = res,
      error: (err) => console.error(err)
    });
  }

  enableEdit() {
    this.isEditing = true;
  }

  save() {
    const dto: UserUpdateDto = {
      id: this.user.id,
      name: this.user.name,
      email: this.user.email,
      phone: this.user.phone,
      role: this.user.role as 'User' | 'Staff',
      accountStatus: this.user.accountStatus
    };

    this.userService.update(this.user.id, dto).subscribe({
      next: (res) => {
        this.user = res;
        this.isEditing = false;
      },
      error: (err) => console.error(err)
    });
  }
  cancel() {
    this.isEditing = false;
    this.ngOnInit(); 
  }
}
