import Login from "./Login.svelte";
import '../../../../node_modules/materialize-css/dist/css/materialize.css'

const app = new Login({
    target: document.getElementById('app'),
})

export default app
