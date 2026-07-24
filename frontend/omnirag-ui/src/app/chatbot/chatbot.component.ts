import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'chatbot-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <h2>Chatbot</h2>
    <p>Chat UI will be implemented here.</p>
  `
})
export class ChatbotComponent {}
