
## Project Description:

The project involves utilizing point cloud data to construct a regular triangulation of a terrain. 
It simulates spheres sliding down the terrain with custom physics that adhere to Newton's second law. 
Additionally, the project simulates rain and creates rivers by generating B-Splines that follow the path of raindrops. 
The simulation extends to spheres floating down the generated river.

## Other Details:

The B-Spline surface employed in the project is bi-quadratic (2nd degree), although it can also be of 3rd or 4th degree.

## Images:

Visualization of pointcloud data in Unity

![image](https://github.com/haldorj/VSIM2023/assets/89477584/8b78780d-be0f-4fdd-85f5-f85593799fb1)

Visualization of terrain in Unity. The blue poins decide the height of the terrain, and are generated from the pointcloud data.

![image](https://github.com/haldorj/VSIM2023/assets/89477584/31bac158-770f-42f5-a701-21fbe4ed6edc)

Water (B-spline curve) left behind by raindrop.

![image](https://github.com/haldorj/VSIM2023/assets/89477584/6e5d0a21-5440-49ab-83d7-1f6b4288291f)

Visualization of extreme weather effect. Balls that are close to a spline curve are swept away by the current.

![image](https://github.com/haldorj/VSIM2023/assets/89477584/f3383085-bc12-427f-9d55-a175bde536d6)

Biquadratic B-spline tensor product surface with 7 control points in the x-direction and 6 control points in the y-direction

![image](https://github.com/haldorj/VSIM2023/assets/89477584/a4df185b-cd94-49d0-9183-d0f908af2e35)

