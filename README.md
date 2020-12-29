# Computational Geometry Unity Library

This library consists of two folders. The idea is that one is for testing purposes and the other folder is the folder you drag into your project. 

Make sure all input coordinates are normalized to range 0-1 to avoid floating point precision issues! There's a "Normalizer" object that will help you normalize and un-normalize. This is not always needed but if you notice that an algorithm doesn't work, try to normalize the input coordinates. 

Some of these algorithms are available in tutorial form here: https://www.habrador.com/tutorials/math/ and here: https://www.habrador.com/tutorials/interpolation/

The code has been tested by using Unity 2018.4 LTS but should work with other versions. 

1. [Intersections](#1-intersections)
2. [Generate mesh](#2-generate-mesh)
3. [Convex hull](#3-convex-hull)
4. [Triangulation](#4-triangulation)
5. [Voronoi diagram](#5-voronoi-diagram)
6. [Polygon clipping](#6-polygon-clipping)
7. [Extrude mesh along curve](#7-extrude-mesh-along-curve)
8. [Deform mesh](#8-deform-mesh)
9. [Other](#9-other)
10. [TODO](#todo)
11. [Major Updates](#major-updates)



## 1. Intersections

### 1.1 2d space (some are also implemented in 3d space)

**Point-triangle**

![Intersection point-triangle](/_media/intersections-point-triangle.png?raw=true)

**Point-polygon.** Suffers from floating point precision issues 

![Intersection point-polygon](/_media/intersections-point-polygon.png?raw=true)

**Triangle-triangle**		

![Intersection triangle-triangle](/_media/intersections-triangle-triangle.png?raw=true)

**AABB-AABB** 			

![Intersection point-triangle](/_media/intersections-aabb-aabb.png?raw=true)

**Line-line** 	

![Intersection line-line](/_media/intersections-line-line.png?raw=true)
				
**Ray-plane** 	

![Intersection ray-plane](/_media/intersections-ray-plane.png?raw=true)
				
**Line-plane** 	

![Intersection line-plane](/_media/intersections-line-plane.png?raw=true)

**Plane-plane**

![Intersection plane-plane](/_media/intersections-plane-plane.png?raw=true)
		
**Point-circle** 	

![Intersection point-circle](/_media/intersections-point-circle.png?raw=true)			


## 2. Generate mesh

**Grid mesh**

![Mesh grid](/_media/mesh-grid.png?raw=true)	

**Mesh shapes:** Arrow, circles, lines

![Mesh shapes](/_media/mesh-shapes.png?raw=true)	


## 3. Convex Hull

A common problem in Computational Geometry is to find the convex hull of a set of points.

### 2d space

![Convex hull 2d space](/_media/convex-hull-2d.png?raw=true)

**Jarvis March.** Is also known as "Gift wrapping"

This is the simplest algorithm. The idea is:
1. Find a point on the hull (which can be the point with the smallest x-coordinate)
2. To find the next point on the hull, you pick a point randomly, and then you test all other points if the other point is to the right of the point between the last point on the hull and the point you picked randomly. If so then this point is a better point. You continue like this until you find no more point to the right. This last point is also on the hull. 

This algorithm may be slow, but it is robust and can easily deal with colinear points. Sometimes it's better to use an algorithm which is easy to understand than a more complicated one.

A visualization of this algorithm can be found here: https://www.youtube.com/watch?v=Z_wcJUgvohc   

**Quickhull.**

A good paper on this algorithm is "Implementing Quickhull" from Valve by Dirk Gregorious. It has images so you can see what's going on. But the idea is:
1. Find a first triangle with three points that are on the hull. Now you can remove all points that are within this triangle
2. For each edge (you start with the triangle's three edges) find the points that are "outside" of this edge
3. For each edge (and by using the points you know are outside of this edge) find the point that is the furthest from this edge. This point is also on the hull. Now the original edge on the triangle has been split into two. Remove all points that are within this new triangle formed by the original edge and the two new edges.   
4. Repeat 2 and 3 for each new edge


### 3d space

![Convex hull 3d space](/_media/convex-hull-3d.png?raw=true)

**Iterative algorithm.** Is very similar to Quickhull. 

1. Like in Quickhull 2d you start by finding a triangle. But this is 3d, so you have to find another point so you get a tetrahedron. 
2. Add all other points one-by-one. If the point is inside the hull you have so-far, ignore it. Otherwise you have to check which triangles are visible from the point and remove them. Then you build triangles to the new point from the border of the triangles you just removed.

A visualization of this algorithm can be found here: https://www.youtube.com/watch?v=Yv2OhCV1BpU  


## 4. Triangulation


### 4.1 Triangulate convex polygon

You have points on a convex hull you want to triangulate. You have four options here if you have colinear points (points on the same line): 
1. Triangulate the convex hull while ignoring the colinear points. The area covered will be the same anyway.
2. Triangulate the convex hull and add the colinear points by splitting triangle edges.
3. Add a point inside of the convex hull.
4. Use the algorithm below called "Triangulate points with 'visible edge' algorithm." 

![Triangulation convex polygon](/_media/triangulation-convex-polygon.png?raw=true)	


### 4.2 Triangulate concave polygon 
	
**Ear Clipping**

Can currently only handle holes - not hole-in-holes. But it has optimizations to get a better looking triangulation. The Ear Clipping algorithm is borrowing ideas from Delaunay triangulation to get nicer looking triangles. So if you encounter problems with this algorithm (because of some edge-case) you can always try the Constrained Delaunay algorithm - they should give the same result. I believe Constrained Delaunay is more robust because you don't have to connect the holes with invisible seams.   

A visualization of this algorithm can be found here: https://www.youtube.com/watch?v=mw8aLh_lPoo

![Ear Clipping](/_media/ear-clipping.png?raw=true)


### 4.3 Triangulate points

**Triangulate points with "visible edge" algorithm.**

You have some points you want to triangulate, you follow the steps:
1. Sort all points in x and then y direction
2. Find the first triangle
3. Add the rest of the sorted points one-by-one and build triangles to visible edges on the existing triangulation. To determine if an edge is visible from the point you build the convex shape from the existing triangles. Then for each edge in the convex hull, you build a triangle with the point. If this triangle is oriented clockwise, the edge is visible and you can add a new triangle.

A visualization of this algorithm can be found here: https://www.youtube.com/watch?v=MkMXKu1m6A4    

![Triangulation visible edges](/_media/triangulation-visible-edges.png?raw=true)	


**Triangulate points with "point-by-point" algorithm.** 

You have some points you want to triangulate, you follow the steps:
1. Generate the convex hull of all points.
2. Triangulate the convex hull with one of several algorithms mentioned above.
3. Add the rest of the points one-by-one by splitting the triangles they end up in into three new triangles.

![Triangulation point-by-point](/_media/triangulation-point-by-point.png?raw=true)		


### 4.4 Delaunay triangulation

**"point-by-point" method** 

You generate a big triangle around all points you want to triangulate. Then you add each point one after the other. The triangle the point ends up in is split into three new triangles. After the split you restore the Delaunay triangulation by flipping edges. When all points have been added you remove the remains of the first big triangle. A visualization of this algorithm can be found here: https://www.youtube.com/watch?v=YNQR5tH-s40  

![Triangulation Delaunay point-by-point](/_media/triangulation-delaunay-point-by-point.png?raw=true)	


**"flip edges" method** 

You triangulate the points by using a "bad" triangulation method (which is in this case either "visible edge" or "point-by-point" from above). Then you go through all edges and check if the edge should be flipped to make a better triangle. When no more edges can be flipped you are done! A visualization of this algorithm can be found: https://www.youtube.com/watch?v=-d7Nb4fxL5s and https://www.youtube.com/watch?v=lR_SzgEkDwk

![Triangulation Delaunay flip edges](/_media/triangulation-delaunay-flip-edges.png?raw=true)	


**Constrained triangulation** 

You add the constraints to the points and generate a Delaunay triangulation by using one of the above methods. Use this triangulation to find which edges interesect with the constraints. Then you flip these edges until they no longer interesect with the constraint. You finally remove the triangles that are "inside" of the constraint. It can handle several holes and a single hull, but not holes-in-holes. If you really need a hole-in-hole you can always run the algorithm twice and then merge the output. A similar algorithm is triangulation by Ear Clipping, but I believe this one is more robust. A visualization of this algorithm can be found here: https://www.youtube.com/watch?v=Z-1ExrWMTfA      

![Triangulation Delaunay constrained](/_media/triangulation-delaunay-constrained.png?raw=true)	


### 4.2 Marching algorithms

**Marching squares:**

Triangulates points in a 2D grid. 

![Marching squares](/_media/marching-squares.png?raw=true)

**Metacircles:**

Metacircles are like Metaballs but in 2D. Is using Marching squares.  

![Metacircles](/_media/metacircles.png?raw=true)



## 5. Voronoi diagram

**From a Delaunay triangulation**

You first generate a Delaunay triangulation by using some method. Then you use the fact that you can get the Voronoi diagram from the Delaunay triangulation. For each Delaunay triangle you generate a circle where the triangle-corners are on the edge of the circle. The center of this circle is a corner in the Voronoi diagram face belonging to the triangle.     

![Voronoi from delaunay](/_media/voronoi-from-delaunay.png?raw=true)	


**From a Delaunay triangulation on a sphere**

To get the Delaunay triangulation of points on a sphere, you just generate the convex hull of those points. To generate the Voronoi diagram in 3d space, the process is the same as in 2d space - except that you need to find the center of a circle given 3 points on the edge of the circle in 3d space.  

![Delaunay on a sphere](/_media/triangulation-delaunay-sphere.png?raw=true)

A visualization of this algorithm can be found here: https://www.youtube.com/watch?v=xAVL4qz_2AE

![Voronoi on a sphere](/_media/voronoi-from-delaunay-sphere.png?raw=true)



## 6. Polygon clipping

**Greiner-Hormann method** 

![Polygon clipping greiner](/_media/polygon-clipping-greiner.png?raw=true)	

**Sutherland-Hodgman method** 

![Polygon clipping sutherland](/_media/polygon-clipping-sutherland.png?raw=true)	



## 7. Extrude mesh along curve

**Catmull-Rom splines**

![Interpolation catmull rom](/_media/interpolation-catmull-rom.png?raw=true)	

**Bezier curves**

* Linear

![Interpolation bezier linear](/_media/interpolation-bezier-linear.png?raw=true)	

* Quadratic

![Interpolation bezier quadratic](/_media/interpolation-bezier-quadratic.png?raw=true)	

* Cubic

![Interpolation bezier cubic](/_media/interpolation-bezier-cubic.png?raw=true)	

**Operations on curves**

* Methods so you can split up the curves into equal steps

* Methods to extrude meshes along the curves. The difficult part here is to find an orientation at a point on the curve, and the following methods are included:

	* Fixed Up
	* Frenet Normal (also known as Frenet Frame)
	* Rotation Minimising Frame (also known as Parallel Transport Frame or Bishop Frame) 

![Interpolation extrude mesh](/_media/interpolation-extrude-mesh.png?raw=true)	



## 8. Deform mesh

The fun part of computational geometry!


**Cut mesh with plane**

If the new meshes are not connected, then it will separate the meshes, so you might end up with more than two meshes after the cut.  

![Cut mesh with plane](/_media/cut-mesh-with-plane.png?raw=true)


**Simplify mesh**

Will generate a mesh similar to the original mesh but with fewer triangles. Is useful for generating LODs, etc. The following algorithms are implemented:

* Iterative pair contraction with the Quadric Error Metric (QEM). This is the most common mesh simplification algorithm. A visualization of this algorithm can be found here: https://www.youtube.com/watch?v=biLY19kuGOs

![Mesh simplification](/_media/mesh-simplification-qem.png?raw=true)



## 9. Other

**Is a triangle oriented clockwise?** 

![Triangle orientation](/_media/other-triangle-orientation.png?raw=true)

**Is a point left, on, or right of vector?** 

![Point vector orientation](/_media/other-point-vector.png?raw=true)

**Is a point left, on, or right of a plane?** Which is the same as the distance to the plane. 

![Point plane orientation](/_media/other-point-plane.png?raw=true)

**Is a quadrilateral convex?** 

![Quadrilateral convex or concave](/_media/other-quadrilateral.png?raw=true)

**Is a point between two other points on the same line?** 

![Is point between points on line segment](/_media/other-point-between-points.png?raw=true)

**Closest point on a line-segment?** 

![Closest point on line segment](/_media/other-closest-point-line-segment.png?raw=true)

**Has passed point?** 

If we are going from A to B, how do we know if we have passed B? Measuring just the distance to B, and say we have passed B if we are closer than x meter to B is not accurate enough!  

![Closest point on line segment](/_media/other-passed-waypoint.png?raw=true)



## TODO

### Stuff to implement

* Convex hull 2d: 
	* Graham scan
	* Iterative
	* Divide and Conquer
* Convex hull 3d:
	* Divide and Conquer
	* Quickhull
* Concave hull 2d:
	* Alpha shape
* Triangulations:
	* Marching cubes
	* Dynamic constrained delaunay triangulation
	* Metaballs by using Marching cubes
	* Marching Squares on a triangulation instead of a grid
	* Triangulation of polygon by "Horizontal decomposition into trapezoids"
	* A way to generate an infinite delaunay triangulation (for terrains etc)
* Deform mesh:
	* Mesh decals
	* Deform mesh after collision like car crash
	* Replicate the algorithm from twitter where you can take a photo of the world and then place the geometry in the photo wherever you want (https://twitter.com/mattstark256)
* Voronoi: 
	* Fortune's algorithm
	* Point-by-point
	* From a set of points in 3d space
* Convex polygon-polygon intersection with SAT
* Irregular grid (Oskar Stålberg style)



## Stuff to fix
 
* Make a test scene to test that the "find which triangle a point is in by triangulation walk" is working
* The Delaunay algorithm "flip edges" might have problems with colinear points
* Remove or clarify the conversions between 2d and 3d
* Ear Clipping with holes-in-holes
* Fix point-in-polygon floating point precision issues (see Geometric Tools for Computer Graphics). Can also be used to improve Ear Clipping because uses the same idea
* Ear Clipping should use half-edge data structure, making it easier to flip triangles and is more standardized
* Improve performance of cut-mesh-with-plane
* Theres a faster way to find which edges intersect with a constraint in Constrained Delaunay



## Major Updates

**2020-12**

* Added cut mesh with plane
* Added 3d convex hull with an iterative algorithm (which is very similar to Quickhull)
* Added 3d voronoi and delaunay on a sphere
* Added mesh simplification by using "Iterative pair contraction with the Quadric Error Metric (QEM)"

**2020-11** 

* Added Marching Squares
* Added Metacircles
* Added extrude mesh along curve 
* Added triangulation by Ear Clipping

**2020-03** 

* Added interpolation such as Bezier curves and Catmull-Rom



## Socials

* Visit my [Portfolio](https://www.habrador.com)

* Follow me on [Twitter](https://twitter.com/eriknordeus)
