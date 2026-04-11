const { contextBridge } = require('electron')
const { ipcRenderer } = require('electron/renderer')

let nextRequestId = 1
const requests = new Map()
const listeners = new Set()

function createRequest() {
  const id = nextRequestId++
  if (nextRequestId > 999999) {
    nextRequestId = 1
  }

  return { id: id, promise: new Promise(resolve => requests.set(id, resolve)) }
}

function completeRequest(id, value) {
  const resolve = requests.get(id)
  if (resolve !== undefined) { 
    requests.delete(id)
    resolve(value)
  }
}

ipcRenderer.on('toFrontend', (_, message) => {
  if (message.response !== undefined) {
    completeRequest(message.response, message.content)
  }
  else {
    listeners.forEach(listener => {
      let isResponded = false
      listener(message.content, responseData => {
        if (message.request === undefined) {
          throw new Error("Cannot respond to a non-request")
        }
        else if (isResponded) {
          throw new Error("Cannot respond to a request more than once")
        }

        isResponded = true
        ipcRenderer.send('toBackend', { response: message.request, content: responseData })
      })
    });
  }
})

contextBridge.exposeInMainWorld('backend', {
  setWidth: value => ipcRenderer.send('setWidth', value),
  setHeight: value => ipcRenderer.send('setHeight', value),
  setDevTools: value => ipcRenderer.send('setDevTools', value),
  setDevToolsShortcut: value => ipcRenderer.send('setDevToolsShortcut', value),
  setFullscreen: value => ipcRenderer.send('setFullscreen', value),
  setFullscreenShortcut: value => ipcRenderer.send('setFullscreenShortcut', value),
  setMavalueimized: value => ipcRenderer.send('setMavalueimized', value),
  setMinimized: value => ipcRenderer.send('setMinimized', value),
  send: value => ipcRenderer.send('toBackend', { content: value}),
  request: async value => {
    const request = createRequest()
    ipcRenderer.send('toBackend', { request: request.id, content: value })
    return await request.promise
  },
  listen: callback => {
    listeners.add(callback)
    return () => listeners.delete(callback)
  }
})