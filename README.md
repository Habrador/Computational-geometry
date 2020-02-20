# Computational Geometry Unity Library

This library consists of two folders. The idea is that one is for testing purposes and the other folder is the folder you drag into your project. 


## Finished


### 1. Geometry test

* Is a triangle oriented clockwise?
* Is a point left, on, or right of vector?
* Is a point left, on, or right of a plane?
* Is a quadrilateral convex?


### 2. Intersection

2d-space:
* Point-triangle				CHECK
* Point-polygon (suffers from floating point precision issues)				CHECK
* Triangle-triangle				CHECK
* AABB-AABB 						CHECK
* Line-line 						CHECK
* Ray-plane						CHECK
* Line-plane					CHECK
* Point-circle 					CHECK


### 3. Convex Hull

* Jarvis March


### 4. Triangulation

* Triangulate convex polygon
* Triangulate points with "visible edge" and "point-by-point" algorithms
* Triangulate with marching squares

#### 4.1 Delaunay triangulation

* Delaunay triangulation - "point-by-point"
* Delaunay triangulation - "triangulate and then flip edges"
* Constrained delaunay triangulation


### 5. Voronoi diagram

* Voronoi from delaunay


### 6. Polygon clipping

* Greiner-Hormann	CHECK
* Sutherland-Hodgman	CHECK


### 7. Generate mesh

* Grid mesh	CHECK


## TODO

* Dynamic constrained delaunay triangulation
* Convex hull: Quick Hull from the Valve paper on Hull
* Convex hull: Gift wrapping
* Marching cubes
* Cut 3d mesh with plane
* Metaballs
* Voronoi with Fortune's algorithm
* Voronoi point-by-point (suffers from floating point precision issues)
* Triangulation concave polygon by ear clipping
* Make sure all algorithms that are in 2d are using 2d data and not 3d to avoid confusion
* Rectangle-rectangle with SAT


## Socials

Follow me on Twitter for more Unity stuff: https://twitter.com/eriknordeus
