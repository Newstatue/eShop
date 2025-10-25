import { useSupportProducts } from "../hooks/useProducts";
import { useState, useRef, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Spinner } from "@/components/ui/spinner";
import { MessageCircle, X, Send, Sparkles } from "lucide-react";
import ReactMarkdown from "react-markdown";

interface Message {
    id: string;
    role: 'user' | 'assistant';
    content: string;
    timestamp: Date;
}

export default function AISupport() {
    const [isOpen, setIsOpen] = useState(false);
    const [messages, setMessages] = useState<Message[]>([
        {
            id: '1',
            role: 'assistant',
            content: '您好！我是AI购物助手，有什么可以帮助您的吗？',
            timestamp: new Date()
        }
    ]);
    const [inputValue, setInputValue] = useState('');
    const [currentQuery, setCurrentQuery] = useState('');
    const messagesEndRef = useRef<HTMLDivElement>(null);

    const { data: answer, isLoading } = useSupportProducts(currentQuery);

    // 自动滚动到底部
    const scrollToBottom = () => {
        messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    };

    useEffect(() => {
        scrollToBottom();
    }, [messages, isLoading]);

    // 处理AI回复
    useEffect(() => {
        if (answer && currentQuery) {
            const newMessage: Message = {
                id: Date.now().toString(),
                role: 'assistant',
                content: answer,
                timestamp: new Date()
            };
            setMessages(prev => [...prev, newMessage]);
            setCurrentQuery('');
        }
    }, [answer, currentQuery]);

    const handleSendMessage = () => {
        if (!inputValue.trim() || isLoading) return;

        const userMessage: Message = {
            id: Date.now().toString(),
            role: 'user',
            content: inputValue,
            timestamp: new Date()
        };

        setMessages(prev => [...prev, userMessage]);
        setCurrentQuery(inputValue);
        setInputValue('');
    };

    const handleKeyDown = (e: React.KeyboardEvent) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            handleSendMessage();
        }
    };

    return (
        <div className="fixed bottom-6 right-6 z-50">
            {/* 主按钮 */}
            {!isOpen && (
                <Button
                    size="lg"
                    className="h-14 w-14 rounded-full shadow-lg transition-all duration-300 hover:scale-110 hover:shadow-xl"
                    onClick={() => setIsOpen(true)}
                >
                    <MessageCircle className="h-6 w-6" />
                </Button>
            )}

            {/* 聊天窗口 */}
            {isOpen && (
                <Card className="w-96 overflow-hidden shadow-2xl transition-all duration-300 animate-in slide-in-from-bottom-5 gap-0 p-0">
                    {/* 头部 */}
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 border-b bg-linear-to-r from-primary/10 via-primary/5 to-background p-4">
                        <div className="flex items-center gap-2">
                            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10">
                                <Sparkles className="h-5 w-5 text-primary" />
                            </div>
                            <div>
                                <h3 className="font-semibold">AI购物助手</h3>
                                <p className="text-xs text-muted-foreground">在线为您服务</p>
                            </div>
                        </div>
                        <Button
                            variant="ghost"
                            size="icon"
                            className="h-8 w-8 rounded-full"
                            onClick={() => setIsOpen(false)}
                        >
                            <X className="h-4 w-4" />
                        </Button>
                    </CardHeader>

                    {/* 消息区域 */}
                    <CardContent className="p-0">
                        <ScrollArea className="h-96 p-4">
                            <div className="space-y-4">
                                {messages.map((message) => (
                                    <div
                                        key={message.id}
                                        className={`flex ${
                                            message.role === 'user' ? 'justify-end' : 'justify-start'
                                        }`}
                                    >
                                        <div
                                            className={`max-w-[80%] rounded-2xl px-4 py-2 ${
                                                message.role === 'user'
                                                    ? 'bg-primary text-primary-foreground'
                                                    : 'bg-muted'
                                            }`}
                                        >
                                            {message.role === 'assistant' ? (
                                                <div className="text-sm leading-relaxed prose prose-sm max-w-none dark:prose-invert prose-p:my-2 prose-p:last:mb-0 prose-ul:my-2 prose-ul:ml-4 prose-ul:list-disc prose-ol:my-2 prose-ol:ml-4 prose-ol:list-decimal prose-li:my-1 prose-strong:font-semibold prose-code:rounded prose-code:bg-muted-foreground/10 prose-code:px-1 prose-code:py-0.5">
                                                    <ReactMarkdown>
                                                        {message.content}
                                                    </ReactMarkdown>
                                                </div>
                                            ) : (
                                                <p className="text-sm leading-relaxed">{message.content}</p>
                                            )}
                                            <p className="mt-1 text-xs opacity-70">
                                                {message.timestamp.toLocaleTimeString('zh-CN', {
                                                    hour: '2-digit',
                                                    minute: '2-digit'
                                                })}
                                            </p>
                                        </div>
                                    </div>
                                ))}
                                {isLoading && (
                                    <div className="flex justify-start">
                                        <div className="flex max-w-[80%] items-center gap-2 rounded-2xl bg-muted px-4 py-2">
                                            <Spinner className="h-4 w-4" />
                                            <span className="text-sm text-muted-foreground">正在思考...</span>
                                        </div>
                                    </div>
                                )}
                                <div ref={messagesEndRef} />
                            </div>
                        </ScrollArea>
                    </CardContent>

                    {/* 输入区域 */}
                    <CardFooter className="border-t p-4">
                        <div className="flex w-full gap-2">
                            <Input
                                placeholder="输入您的问题..."
                                value={inputValue}
                                onChange={(e) => setInputValue(e.target.value)}
                                onKeyDown={handleKeyDown}
                                disabled={isLoading}
                                className="flex-1"
                            />
                            <Button
                                size="icon"
                                onClick={handleSendMessage}
                                disabled={!inputValue.trim() || isLoading}
                            >
                                <Send className="h-4 w-4" />
                            </Button>
                        </div>
                    </CardFooter>
                </Card>
            )}
        </div>
    );
}