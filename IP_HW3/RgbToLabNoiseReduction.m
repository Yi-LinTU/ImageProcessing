function RgbToLabNoiseReduction(x)
    
    F = imread(x);
    F = im2double(F);
    Lab = rgb2lab(F);
    L = Lab(:,:,1);
    A = Lab(:,:,2);
    B = Lab(:,:,3);
    
    [height, width] = size(F);
    M = height * width;
    
    figure('Name','L*a*b Noise Reduction','NumberTitle','off'),
    subplot(3,4,1);imshow(x),title('Original RGB Image');
    subplot(3,4,2);imshow(Lab(:,:,1),[0,100]),title('L');
    subplot(3,4,6);imshow(Lab(:,:,2),[-128,127]),title('a');
    subplot(3,4,10);imshow(Lab(:,:,3),[-128,127]),title('b');
    hold on;
    
    % Enhancement
    h = fspecial('gaussian',5,1);
    Lab(:,:,1) = imfilter(L,h);
    Lab(:,:,2) = imfilter(A,h);
    Lab(:,:,3) = imfilter(B,h);
    

    subplot(3,4,3);imshow(Lab(:,:,1),[0,100]),title('L Noise Reduction');
    subplot(3,4,7);imshow(Lab(:,:,2),[-128,127]),title('a Noise Reduction');
    subplot(3,4,11);imshow(Lab(:,:,3),[-128,127]),title('b Noise Reduction');
    hold on;
    
    Output_Lab = lab2rgb(Lab);
    subplot(3,4,4);imshow(Output_Lab),title('RGB Image - L*a*b Noise Reduction');
    hold off;

%     figure('Name','L*a*b Noise Reduction Enhancement','NumberTitle','off'),
%     subplot(1,2,1);imshow(F),title('Original RGB Image');
%     subplot(1,2,2);imshow(Output_Lab),title('Enhancement L*a*b Image');


end