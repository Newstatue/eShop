import { useOidc } from "@axa-fr/react-oidc";

export default function Login() {
    const { login } = useOidc();

    return (
        <div className="login">
            <button onClick={() => login()}>Login</button>
        </div>
    );
}