import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { environment } from '../../environments/environment';

@Component({
    selector: 'app-maze-upload',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './maze-upload.component.html',
    styleUrl: './maze-upload.component.css'
})
export class MazeUploadComponent {
    selectedFile: File | null = null;
    message: string = '';
    isUploading = false;

    constructor(private http: HttpClient) { }

    onFileSelected(event: any) {
        this.selectedFile = event.target.files[0] ?? null;
        this.message = '';
    }

    onUpload() {
        if (!this.selectedFile) {
            this.message = 'Please select a file first.';
            return;
        }

        this.isUploading = true;
        this.message = 'Uploading...';

        const formData = new FormData();
        formData.append('file', this.selectedFile);

        this.http.post<any>(`${environment.apiBaseUrl}/Mazes/upload`, formData).subscribe({
            next: (response) => {
                this.message = 'Upload successful! Maze ID: ' + response.id;
                this.isUploading = false;
                this.selectedFile = null;
                // Optional: clear file input
                const fileInput = document.getElementById('fileInput') as HTMLInputElement;
                if (fileInput) fileInput.value = '';
            },
            error: (error) => {
                console.error('Upload failed', error);
                this.message = 'Upload failed: ' + (error.error || error.message);
                this.isUploading = false;
            }
        });
    }
}
