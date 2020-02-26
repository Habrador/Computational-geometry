# Computational Geometry Unity Library

This library consists of two folders. The idea is that one is for testing purposes and the other folder is the folder you drag into your project. 


## Finished


### 1. Intersection

2d-space:
* Point-triangle 	
* Point-polygon (suffers from floating point precision issues) 
* Triangle-triangle		
* AABB-AABB 			
* Line-line 					
* Ray-plane 					
* Line-plane 			
* Point-circle 				


### 2. Generate mesh

* Grid mesh	


### 3. Convex Hull

* Jarvis March (Gift wrapping)


### 4. Triangulation

* Triangulate convex polygon - is working with colinear points
* Triangulate points with "visible edge" algorithm - is maybe working with colinear points
* Triangulate points with "point-by-point" algorithm - is working with colinear points (because Triangulate convex polygon is working with colinear points)
* Triangulate with marching squares

#### 4.1 Delaunay triangulation

* Delaunay triangulation - "point-by-point" 
* Delaunay triangulation - "triangulate and then flip edges" 
* Constrained delaunay triangulation 


### 5. Voronoi diagram

* Voronoi from delaunay


### 6. Polygon clipping

* Greiner-Hormann 
* Sutherland-Hodgman 


### 7. Other

* Is a triangle oriented clockwise? 
* Is a point left, on, or right of vector? 
* Is a point left, on, or right of a plane? Which is the same as the distance to the plane. 
* Is a quadrilateral convex? 
* Is a point between two other points on the same line? 
* Closest point on a line-segment? 


## TODO

### Algorithms to implement

* Dynamic constrained delaunay triangulation
* Convex hull: Quickhull from the Valve paper
* Convex hull: Graham scan
* Marching cubes
* Cut 3d mesh with plane
* Metaballs
* Voronoi with Fortune's algorithm
* Triangulation concave polygon by ear clipping
* Rectangle-rectangle with SAT

### Stuff to fix

* The triangulation by splitting triangles, there's a small problem if a vertex ends up on the edge and is not inside the triangle
* Voronoi point-by-point (suffers from floating point precision issues)
* Point-polygon intersection floating point precision issues
* Make sure all input values in all algorithms are normalized to 0,1 to avoid floating point precision issues


## Socials

Follow me on Twitter for more Unity stuff: https://twitter.com/eriknordeus
