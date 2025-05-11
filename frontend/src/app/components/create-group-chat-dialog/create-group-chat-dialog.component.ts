import { Component, Inject, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatListModule, MatSelectionList } from '@angular/material/list';
import { CommonModule } from '@angular/common';
import { User } from '../../models/user.model';
import { UserService } from '../../services/user.service';
import { 
  MatChipsModule, 
  MatChipGrid, 
  MatChipRow, 
  MatChipRemove,
} from '@angular/material/chips';

@Component({
  selector: 'app-create-group-chat-dialog',
  templateUrl: './create-group-chat-dialog.component.html',
  styleUrls: ['./create-group-chat-dialog.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatChipsModule,
    MatIconModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatListModule,
    MatChipGrid,
    MatChipRow,
    MatChipRemove
  ]
})
export class CreateGroupChatDialogComponent {
  @ViewChild('userList') userList!: MatSelectionList;
  
  groupForm: FormGroup;
  users: User[] = [];
  searchControl = new FormControl('');
  selectedUsers: User[] = [];

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<CreateGroupChatDialogComponent>,
    private userService: UserService,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.groupForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
    });
    this.loadUsers();
  }

  loadUsers() {
    this.userService.getUsers().subscribe(users => {
      this.users = users.filter(user => 
        user.id !== localStorage.getItem('userId')
      );
    });
  }

  toggleUserSelection(user: User) {
    const index = this.selectedUsers.findIndex(u => u.id === user.id);
    if (index === -1) {
      this.selectedUsers.push(user);
      if (this.userList) {
        const option = this.userList.options.find(opt => opt.value.id === user.id);
        if (option) {
          option.selected = true;
        }
      }
    } else {
      this.selectedUsers.splice(index, 1);
      if (this.userList) {
        const option = this.userList.options.find(opt => opt.value.id === user.id);
        if (option) {
          option.selected = false;
        }
      }
    }
  }
  onSelectionListChange(event: any) {
  this.selectedUsers = event.source.selectedOptions.selected.map((option: any) => option.value);
}

  isUserSelected(user: User): boolean {
    return this.selectedUsers.some(u => u.id === user.id);
  }

  onSubmit() {
    if (this.groupForm.valid && this.selectedUsers.length > 0) {
      this.dialogRef.close({
        name: this.groupForm.get('name')?.value,
        users: this.selectedUsers
      });
    }
  }

  onCancel() {
    this.dialogRef.close();
  }

  get filteredUsers() {
    const searchTerm = this.searchControl.value?.toLowerCase() || '';
    return this.users.filter(user => 
      user.userName.toLowerCase().includes(searchTerm)
    );
  }
}
