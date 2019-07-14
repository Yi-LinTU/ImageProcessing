function RgbToLabLightness(x)
    
    F = imread(x);
    F = im2double(F);
    Lab = rgb2lab(F);
    [height, width] = size(F);
    M = height * width;
    
    figure('Name','L*a*b Lightness Enhancement','NumberTitle','off'),
    subplot(3,4,1);imshow(x),title('Original RGB Image');
    subplot(3,4,2);imshow(Lab(:,:,1),[0,100]),title('L');
    subplot(3,4,6);imshow(Lab(:,:,2),[-128,127]),title('a');
    subplot(3,4,10);imshow(Lab(:,:,3),[-128,127]),title('b');
    hold on;
    
    % L Enhancement
    sum_of_L = sum(Lab(:,:,1));
    sum_of_L = sum(sum_of_L) / M;
    gamma = sum_of_L / 10;
    
    if gamma < 1
        gamma = gamma * 1.6;
     
    elseif gamma > 1
        gamma = gamma / 1.6;
    end
    
    gamma
    Lab(:,:,1) = 100 * ( (Lab(:,:,1) / 100).^ gamma);

    subplot(3,4,3);imshow(Lab(:,:,1),[0,100]),title('L Enhancement');
    subplot(3,4,7);imshow(Lab(:,:,2),[-128,127]),title('a');
    subplot(3,4,11);imshow(Lab(:,:,3),[-128,127]),title('b');
    hold on;
    
    Output_Lab = lab2rgb(Lab);
    subplot(3,4,4);imshow(Output_Lab),title('RGB Image - L*a*b Enhancement');
    hold off;

%     figure('Name','L*a*b Lightness Enhancement','NumberTitle','off'),
%     subplot(1,2,1);imshow(F),title('Original RGB Image');
%     subplot(1,2,2);imshow(Output_Lab),title('Enhancement L*a*b Image');

end