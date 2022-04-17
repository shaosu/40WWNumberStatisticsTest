# 4 Billion Find Missing Numbers

The problem comes from the problems circulating among the Internet. 
The basic idea is to turn the big into the small and break them one by one. 
Test various parameters for the best parameters.

## Features

- The quantity and grouping quantity can be set manually
- Multi thread and single thread methods are implemented at the same time
- CSV log output

## Example

   App1.exe 40000 64  

## Result

- Less than 40000, the single thread is fast
- Grouping is small and faster
- Single cycle faster
- Higher than 400 million, the grouping needs to be less than 64

## Optimum Parameters

- Higher than 40000:Multithreading, 64 Grouping, Single loop
- Less than 40000:Single thread,64 Grouping,Single loop

