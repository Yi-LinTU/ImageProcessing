function RgbToHsiNoiseReduction(x)

    F = imread(x);
    F = im2double(F);
    r = F(:,:,1);
    g = F(:,:,2);
    b = F(:,:,3);
    th = acos((0.5*((r-g)+(r-b)))./((sqrt((r-g).^2+(r-b).*(g-b)))+eps));
    H = th;
    H(b > g) = 2*pi-H(b>g);
    H = H/(2*pi);
    S = 1-3.*(min(min(r,g),b))./(r+g+b+eps);
    I = (r+g+b)/3;
    hsi = cat(3,H,S,I);
    HN = H*2*pi;
    
    h = fspecial('gaussian',5,1.2);
    HN = imfilter(HN,h);
    SN = imfilter(S,h);
    IN = imfilter(I,h);
  
    HN = HN/(2*pi);
    
    figure('Name','HSI Noise Reduction Enhancement','NumberTitle','off'),
    subplot(3,4,1);imshow(F),title('Original RGB Image');
    subplot(3,4,5);imshow(hsi),title('Original HSI Image');
    subplot(3,4,2);imshow(H),title('Hue');
    subplot(3,4,6);imshow(S),title('Saturation');
    subplot(3,4,10);imshow(I),title('Intensity'); 
    
    RV=cat(3,H,S,I);
    C=hsitorgb(RV);
    subplot(3,4,3);imshow(C),title('RGB Hue');
    RV=cat(3,H,SN,I);
    C=hsitorgb(RV);
    subplot(3,4,7);imshow(C),title('RGB Image - Saturation Noise Reduction');
    RV=cat(3,H,S,IN);
    C=hsitorgb(RV);
    subplot(3,4,11);imshow(C),title('RGB Image - Intensity Noise Reduction');
    RV=cat(3,H,SN,IN);
    C=hsitorgb(RV);
    subplot(3,4,4);imshow(C),title('RGB Image - HSI Noise Reduction');

    
%     figure('Name','HSI Noise Reduction Enhancement','NumberTitle','off'),
%     subplot(1,2,1);imshow(F),title('Original RGB Image');
%     subplot(1,2,2);imshow(C),title('Enhancement HSI Image');

    
   
end
function C = hsitorgb(hsi)
HV = hsi(:,:,1)*2*pi;
SV = hsi(:,:,2);
IV = hsi(:,:,3);

R = zeros(size(HV));
G = zeros(size(HV));
B = zeros(size(HV));
    
%RG Sector
id = find((0<=HV)& (HV<2*pi/3));
B(id) = IV(id).*(1-SV(id));
R(id) = IV(id).*(1+SV(id).*cos(HV(id))./cos(pi/3-HV(id)));
G(id) = 3*IV(id)-(R(id)+B(id));
%BG Sector
id = find((2*pi/3<=HV)& (HV<4*pi/3));
R(id) = IV(id).*(1-SV(id));
G(id) = IV(id).*(1+SV(id).*cos(HV(id)-2*pi/3)./cos(pi-HV(id)));
B(id) = 3*IV(id)-(R(id)+G(id));
%BR Sector
id = find((4*pi/3<=HV)& (HV<2*pi));
G(id) = IV(id).*(1-SV(id));
B(id) = IV(id).*(1+SV(id).*cos(HV(id)-4*pi/3)./cos(5*pi/3-HV(id)));
R(id) = 3*IV(id)-(G(id)+B(id));

h = fspecial('gaussian',5,1);
R = imfilter(R,h);
G = imfilter(G,h);
B = imfilter(B,h);

C = cat(3,R,G,B);
C = max(min(C,1),0);
end
