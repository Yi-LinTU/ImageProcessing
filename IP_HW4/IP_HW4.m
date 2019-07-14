clear all;
clc;

[file,path] = uigetfile('*.jpg;*.png;*.bmp',...
               'Select an Image File');
filePath = strcat(path, file)
F = imread(filePath);
F = im2double(F);
[m n] = size(F);

% Sobel --------------------------------------------------------
Image = F;
sbl_H = [-1 0 1 ; -2 0 2; -1 0 1];
sbl_V = [-1 -2 -1 ; 0 0 0 ; 1 2 1];
Output_sbl_H = imfilter(Image, sbl_H);
Output_sbl_V = imfilter(Image, sbl_V);

magnitude = sqrt(Output_sbl_H.^2 + Output_sbl_V.^2);
mean = sum(magnitude);
mean = sum(mean) / (m*n);

if mean < 0.8
    mean = mean * 1.6;
end

thresh = magnitude < mean;
magnitude(thresh) = 0;

figure('Name','Sobel Edge Detection','NumberTitle','off'),
subplot(1,2,1);imshow(F),title('Original Image');
subplot(1,2,2);imshow(magnitude),title('Sobel edge');
% Sobel --------------------------------------------------------

% LoG --------------------------------------------------------
Image = F;
Log_filter = fspecial('log',3,4.0);
img_LOG = imfilter(Image, Log_filter);

figure('Name','Laplacian of Guassian Edge Detection','NumberTitle','off'),
subplot(1,3,1);imshow(F),title('Original Image');
subplot(1,3,2);imshow(img_LOG,[]),title('Laplacian of Guassian edge');

mean = sum(img_LOG);
mean = sum(mean) / (m*n);

thresh = img_LOG < mean;
img_LOG(thresh) = 0;
subplot(1,3,3);imshow(img_LOG,[]),title('Thresolded Laplacian of Guassian edge');
% LoG --------------------------------------------------------


