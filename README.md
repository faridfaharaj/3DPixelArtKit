
# A 3D to Pixel art renderer for Unity 3D



A package to make **3D objects** render as **hand drawn pixel art** sprites!

<img width="639" height="480" alt="ezgif-3d62c2da8950cec4" src="https://github.com/user-attachments/assets/55e048e3-d515-426b-b66e-43976f50073d" />


## How?

This uses depth and depth normal textures to render outlines onto the screen that give off that nice pixel art look, by downscaling the output render and placing it in front of the project's camera, to then take both:

**Depth Camera texture**

<img width="918" height="518" alt="image" src="https://github.com/user-attachments/assets/b1608c68-9efb-4d0c-bd19-b6e0d16f6be9" />


and


**Normals Camera texture**

<img width="851" height="468" alt="image" src="https://github.com/user-attachments/assets/ff0733ae-980b-4f74-95e9-bebdfa795766" />


These are taken to a hlsl custom shader to compare the difference of depth of each texel and apply a configurable edge detection algorithm in order to get only outlines **inside** the object to prevent spilling
same is done with normals texture with the difference of taking normal direction between pixels to determine concave/convex relationships instead of depth. 

Both outlines are combined and processed with the object own color (though this is configurable)

A script can also be added to moving objects in order to prevent the "jittery effect" of a downscaled render by offsetting objects by rounding their position via world position to texel position translation


<img width="800" height="448" alt="ezgif-3062ed6731459687" src="https://github.com/user-attachments/assets/c8111002-5057-47a3-913b-ae49745fbe15" />


And thats it! Perfect pixel art effect that works everywhere at any chosen resolution
