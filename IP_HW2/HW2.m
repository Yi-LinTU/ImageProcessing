clc;
clear all;
close all;

% Spatial Domain ----------------------------------------------------------

img_1 = imread('blurry_moon.tif');
img_2 = imread('skeleton_orig.bmp');
img_2 = rgb2gray(img_2);
M1 = size(img_1, 1); N1 = size(img_1, 2);
M2 = size(img_2, 1); N2 = size(img_2, 2);

figure('Name','Spatial Domain','NumberTitle','off'); 
suptitle('Spatial Domain');
subplot(2,4,1);imshow(img_1); title('Original image');
subplot(2,4,5);imshow(img_2); title('Original image');

% Laplacian sharpening ----------------------------------------------------
k = 1.4;
lap = (1/9) * [1 1 1 ; 1 -(9*k-1) 1 ; 1 1 1];
resp_1 = conv2(img_1, lap);
resp_2 = conv2(img_2, lap);
LS_sharpened_1 = double(img_1) - resp_1(1:M1,1:N1);
LS_sharpened_2 = double(img_2) - resp_2(1:M2,1:N2);
subplot(2,4,2);imshow(uint8(LS_sharpened_1),[]); title('Laplacian');
subplot(2,4,6);imshow(uint8(LS_sharpened_2),[]); title('Laplacian');

% Unsharp masking ---------------------------------------------------------
k = 1.4;
um = (1/9) * [-1 -1 -1 ; -1 9*k-1 -1 ; -1 -1 -1];

UM_1 = conv2(img_1, um);
UM_2 = conv2(img_2, um);

UM_sharpened_1 = double(img_1) + UM_1(1:M1,1:N1);
UM_sharpened_2 = double(img_2) + UM_2(1:M2,1:N2);
subplot(2,4,3);imshow(uint8(UM_sharpened_1),[]); title('Unsharp');
subplot(2,4,7);imshow(uint8(UM_sharpened_2),[]); title('Unsharp');

% High-boost --------------------------------------------------------------
alpha = 1.4;
hb = (1/9) * [-1 -1 -1 ; -1 9*alpha-1 -1 ; -1 -1 -1];

HB_1 = conv2(img_1, hb);
HB_2 = conv2(img_2, hb);

HB_sharpened_1 = alpha*double(img_1) + HB_1(1:M1,1:N1);
HB_sharpened_2 = alpha*double(img_2) + HB_2(1:M2,1:N2);
subplot(2,4,4);imshow(uint8(HB_sharpened_1),[]); title('High-boost');
subplot(2,4,8);imshow(uint8(HB_sharpened_2),[]); title('High-boost');
% Spatial Domain ----------------------------------------------------------



% Frequency Domain --------------------------------------------------------
img_1 = imread('blurry_moon.tif');
img_2 = imread('skeleton_orig.bmp');
img_2 = rgb2gray(img_2);

M1 = size(img_1, 1); N1 = size(img_1, 2);
M2 = size(img_2, 1); N2 = size(img_2, 2);

figure('Name','Frequency Domain','NumberTitle','off'); 
suptitle('Frequency Domain');
subplot(2,4,1);imshow(img_1); title('Original image');
subplot(2,4,5);imshow(img_2); title('Original image');

% Laplacian ---------------------------------------------------------------
%// image 1
k = 1.4;
lap = (1/9) * [1 1 1 ; 1 -(9*k-1) 1 ; 1 1 1];
PD = paddedsize(size(img_1));
Hp = fft2(double(lap), PD(1), PD(2));
Fp = fft2(double(img_1), PD(1), PD(2));
F = Hp.*Fp;
f = ifft2(F);
f = f(1:M1, 1:N1);
LS_sharpened_1 = double(img_1) - f;
subplot(2,4,2);imshow(uint8(LS_sharpened_1),[]); title('Laplacian');

%// image 2
PD = paddedsize(size(img_2));
Hp = fft2(double(lap), PD(1), PD(2));
Fp = fft2(double(img_2), PD(1), PD(2));
F = Hp.*Fp;
f = ifft2(F);
f = f(1:M2, 1:N2);
LS_sharpened_2 = double(img_2) - f;
subplot(2,4,6);imshow(uint8(LS_sharpened_2),[]); title('Laplacian');

% Unsharp masking ---------------------------------------------------------
%// image 1
k = 1.4;
um = (1/9) * [-1 -1 -1 ; -1 9*k-1 -1 ; -1 -1 -1];
PD = paddedsize(size(img_1));
Hp = fft2(double(um), PD(1), PD(2));
Fp = fft2(double(img_1), PD(1), PD(2));

F = Hp.*Fp;
f = ifft2(F);
f = f(1:M1, 1:N1);
UM_sharpened_1 = double(img_1) + f;
subplot(2,4,3);imshow(uint8(UM_sharpened_1),[]); title('Unsharp');

%// image 2
PD = paddedsize(size(img_2));
Hp = fft2(double(um), PD(1), PD(2));
Fp = fft2(double(img_2), PD(1), PD(2));

F = Hp.*Fp;
f = ifft2(F);
f = f(1:M2, 1:N2);
UM_sharpened_2 = double(img_2) + f;
subplot(2,4,7);imshow(uint8(UM_sharpened_2),[]); title('Unsharp');

% High-boost ---------------------------------------------------------
alpha = 1.4;
hb = (1/9) * [-1 -1 -1 ; -1 9*alpha-1 -1 ; -1 -1 -1];

%// image 1
PD = paddedsize(size(img_1));
Hp = fft2(double(hb), PD(1), PD(2));
Fp = fft2(double(img_1), PD(1), PD(2));

F = Hp.*Fp;
f = ifft2(F);
f = f(1:M1, 1:N1);
HB_sharpened_1 = alpha * double(img_1) + f;
subplot(2,4,4);imshow(uint8(HB_sharpened_1),[]); title('High-boost');

%// image 2
PD = paddedsize(size(img_2));
Hp = fft2(double(hb), PD(1), PD(2));
Fp = fft2(double(img_2), PD(1), PD(2));

F = Hp.*Fp;
f = ifft2(F);
f = f(1:M2, 1:N2);
HB_sharpened_2 = alpha * double(img_2) + f;
subplot(2,4,8);imshow(uint8(HB_sharpened_2),[]); title('High-boost');

% Frequency Domain --------------------------------------------------------



