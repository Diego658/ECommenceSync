import { config } from '../constants/constants'
import { authHeader } from '../_helpers';
import UserSesion from '../utils/userSesion';

//import { renderSync } from 'node-sass';

export const BlobService = {
  getBlobData
}


async function getBlobData(blobId) {
  const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
  const requestOptions = { method: 'GET', headers: authHeader() };
  const request = new Request(`${config.url.API_URL}/api/Blob/GetBlobData?configuracionId=${configuracionId}&blobId=${blobId}`, requestOptions);
  if (window.caches === undefined ) {
    const response = await fetch(request);
    const blob = await response.blob();
    return blob;
  }
  else {
    const cache = await caches.open('images-cache');
    let response = await cache.match(request);
    if (response === undefined) {
      response = await fetch(request);
      var buffer = await response.arrayBuffer();
      const responseStream = new Response(buffer);
      await cache.put(request, responseStream);
    }
    response = await cache.match(request);
    const blob = await response.blob();
    return blob;
  }



  // console.log(response);
  // return response;
  // const readableStream = ()=>{
  //   const reader =  response.body.getReader();
  //   return new ReadableStream({
  //     start(controller) {
  //       return pump();
  //       function pump() {
  //         return reader.read().then(({ done, value }) => {
  //           // When no more data needs to be consumed, close the stream
  //           if (done) {
  //             controller.close();
  //             return;
  //           }
  //           // Enqueue the next data chunk into our target stream
  //           controller.enqueue(value);
  //           return pump();
  //         });
  //       }
  //     }
  //   })
  // };

  // return readableStream
  // .then(stream => new Response(stream))
  //   .then(response => response.blob())
  //   .then(blob => {
  //     return blob;
  //     // const blobObj = {};
  //     // blobObj.localUrl = URL.createObjectURL(blob);
  //     // blobObj.sizeBlob = blob.size;
  //     // ls.set(localStorageKey, JSON.stringify(blobObj));
  //     // return blobObj;
  //   });

  // const localStorageKey = `image_${blobId}`;
  // const localData = ls.get(localStorageKey) || null;
  // if (localData) {
  //   return JSON.parse(localData);
  // }

  // return fetch(`${config.url.API_URL}/api/Blob/GetBlobData?configuracionId=${configuracionId}&blobId=${blobId}`, requestOptions)
  //   .then(response => {
  //     const reader = response.body.getReader();
  //     return new ReadableStream({
  //       start(controller) {
  //         return pump();
  //         function pump() {
  //           return reader.read().then(({ done, value }) => {
  //             // When no more data needs to be consumed, close the stream
  //             if (done) {
  //               controller.close();
  //               return;
  //             }
  //             // Enqueue the next data chunk into our target stream
  //             controller.enqueue(value);
  //             return pump();
  //           });
  //         }
  //       }
  //     })
  //   })
  //   .then(stream => new Response(stream))
  //   .then(response => response.blob())
  //   .then(blob => {
  //     const blobObj = {};
  //     blobObj.localUrl = URL.createObjectURL(blob);
  //     blobObj.sizeBlob = blob.size;
  //     ls.set(localStorageKey, JSON.stringify(blobObj));
  //     return blobObj;
  //   });
}