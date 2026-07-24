import { Routes } from '@angular/router';
import { DocumentsComponent } from './documents/documents.component';
import { ChatbotComponent } from './chatbot/chatbot.component';
import { AuthGuard } from './core/auth.guard';
import { LoginComponent } from './login/login.component';
import { DashboardComponent } from './dashboard/dashboard.component';

export const routes: Routes = [
	{ path: '', redirectTo: 'dashboard', pathMatch: 'full' },
	{ path: 'login', component: LoginComponent },
	{ path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard] },
	{ path: 'documents', component: DocumentsComponent, canActivate: [AuthGuard] },
	{ path: 'chatbot', component: ChatbotComponent, canActivate: [AuthGuard] }
];
