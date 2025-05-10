export type LoginResponse = {
    token: string;
    avatarPath: string;
    email: string;
    userName: string;
    userId : string;
  }
  
  export type RegisterResponse = {
    id: string;
    userName: string;
    email: string;
    avatarPath: string;
  }
  
  export type LoginRequest = {
    userNameOrEmail: string;
    password: string;
  }
  
export type RegisterRequest = {
  userName: string;
  email: string;
  password: string;
  avatarFile?: File;  // Optional since we'll use default.png if not provided
}
