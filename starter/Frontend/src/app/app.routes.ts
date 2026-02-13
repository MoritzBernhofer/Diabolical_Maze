import { Routes } from '@angular/router';
import { MazeListComponent } from './maze-list/maze-list.component';
import { MazeUploadComponent } from './maze-upload/maze-upload.component';

export const routes: Routes = [
    { path: 'mazes', component: MazeListComponent },
    { path: 'upload', component: MazeUploadComponent },
    { path: '', redirectTo: '/mazes', pathMatch: 'full' }
];
