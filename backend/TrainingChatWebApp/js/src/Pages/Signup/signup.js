import Signup from "./Signup.svelte";
import '../../../../node_modules/materialize-css/dist/css/materialize.css'

const app = new Signup({
    target: document.getElementById('app'),
})

export default app
