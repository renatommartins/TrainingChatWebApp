import Chat from "./Chat.svelte";
import '../../../../node_modules/materialize-css/dist/css/materialize.css'

const app = new Chat({
    target: document.getElementById('app'),
})

export default app

