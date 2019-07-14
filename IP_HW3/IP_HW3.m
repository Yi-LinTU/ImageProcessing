clear all;
clc;

img_01 = 'aloe.jpg';
img_02 = 'church.jpg';
img_03 = 'house.jpg';
img_04 = 'img69.jpg';
img_05 = 'kitchen.jpg';
currentFolder = pwd;
currentFolder = strcat(pwd,'\Image\');
[file,path] = uigetfile('*.jpg;*.png;*.bmp',...
               'Select an Image File');
filePath = strcat(path, file)
F = imread(filePath);
F = im2double(F);

choice = input('1:Lightness Enhancement\n2:Noise Reduction Enhancement\n0:Terminate\n>> Enter your choice :');
switch choice
    case 1 % Histogram Equalization
        % RGB Space
        r = F(:,:,1);
        g = F(:,:,2);
        b = F(:,:,3);
        Eq_R = adapthisteq(r);
        Eq_G = adapthisteq(g);
        Eq_B = adapthisteq(b);
        Eq_rgb = F;
        Eq_rgb(:,:,1) = Eq_R;
        Eq_rgb(:,:,2) = Eq_G;
        Eq_rgb(:,:,3) = Eq_B;
        
        figure('Name','RGB Lightness Enhancement','NumberTitle','off'),
        subplot(3,4,1);imshow(F),title('Original RGB Image');
        subplot(3,4,2);imshow(r),title('Red');
        subplot(3,4,6);imshow(g),title('Green');
        subplot(3,4,10);imshow(b),title('Blue'); 
        subplot(3,4,3);imshow(Eq_R),title('Red Equalized');
        subplot(3,4,7);imshow(Eq_G),title('Green Equalized');
        subplot(3,4,11);imshow(Eq_B),title('Blue Equalized');
        subplot(3,4,4);imshow(Eq_rgb),title('RGB Equalized');
        
%         figure('Name','RGB Lightness Enhancement','NumberTitle','off'),
%         subplot(1,2,1);imshow(F),title('Original RGB Image');
%         subplot(1,2,2);imshow(Eq_rgb),title('Enhancement RGB Image');
        
        % HSI Space
        RgbToHsiLightness( filePath );
        
        % L*a*b Space
        RgbToLabLightness(filePath);
        
    case 2 % Noise Reduction
        % RGB Space
        Output_Noise_img = F;
        r = F(:,:,1);
        g = F(:,:,2);
        b = F(:,:,3);
        h = fspecial('gaussian',5,1);
        Output_Noise_img(:,:,1) = imfilter(r,h);
        Output_Noise_img(:,:,2) = imfilter(g,h);
        Output_Noise_img(:,:,3) = imfilter(b,h);

        figure('Name','RGB Noise Reduction Enhancement','NumberTitle','off'),
        subplot(3,4,1);imshow(F),title('Original RGB Image');
        subplot(3,4,2);imshow(r),title('Red');
        subplot(3,4,6);imshow(g),title('Green');
        subplot(3,4,10);imshow(b),title('Blue'); 
        subplot(3,4,3);imshow(Output_Noise_img(:,:,1)),title('Red Noise Reduction');
        subplot(3,4,7);imshow(Output_Noise_img(:,:,2)),title('Green Noise Reduction');
        subplot(3,4,11);imshow(Output_Noise_img(:,:,3)),title('Blue Noise Reduction');
        subplot(3,4,4);imshow(Output_Noise_img),title('RGB Noise Reduction');
        
%         figure('Name','RGB Noise Reduction Enhancement','NumberTitle','off'),
%         subplot(1,2,1);imshow(F),title('Original RGB Image');
%         subplot(1,2,2);imshow(Output_Noise_img),title('Enhancement RGB Image');


        % HSI Space
        RgbToHsiNoiseReduction( filePath );
        
        % L*a*b Space
        RgbToLabNoiseReduction( filePath );
    
    case 0 % byebye
        fprintf('Good bye\n');
        
    otherwise
        display('Wrong choice');
end
    
    
    
